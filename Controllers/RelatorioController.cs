using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RelatoriosTI.API.Data;
using RelatoriosTI.API.Models;
using RelatoriosTI.API.Services;

namespace RelatoriosTI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RelatorioController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly PdfService _pdfService;

    public RelatorioController(AppDbContext context, PdfService pdfService)
    {
        _context = context;
        _pdfService = pdfService;
    }

    [HttpPost("gerar")]
    public async Task<ActionResult<DadosRelatorio>> GerarRelatorio([FromBody] RelatorioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.HotelNome))
            return BadRequest(new { message = "Nome do hotel é obrigatório" });

        if (request.Tickets == null || request.Tickets.Count == 0)
            return BadRequest(new { message = "É necessário adicionar pelo menos um ticket" });

        // Verificar se o usuário existe
        if (request.CriadoPorUsuarioId > 0)
        {
            var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == request.CriadoPorUsuarioId);
            if (!usuarioExiste)
                return BadRequest(new { message = "Usuário inválido" });
        }

        var agora = DateTime.UtcNow;
        
        // Validar e processar datas de renovação - GARANTIR UTC
        DateTime dataRenovacaoAntivirus = request.DataRenovacaoAntivirus;
        DateTime dataRenovacaoOffice365 = request.DataRenovacaoOffice365;
        
        // Se as datas são DateTime.MinValue (padrão), usar data atual + 1 ano
        if (dataRenovacaoAntivirus == DateTime.MinValue || dataRenovacaoAntivirus.Kind == DateTimeKind.Unspecified)
            dataRenovacaoAntivirus = agora.AddYears(1);
        else
            // Converter para UTC se não for
            dataRenovacaoAntivirus = dataRenovacaoAntivirus.Kind == DateTimeKind.Utc 
                ? dataRenovacaoAntivirus 
                : DateTime.SpecifyKind(dataRenovacaoAntivirus.ToUniversalTime(), DateTimeKind.Utc);
        
        if (dataRenovacaoOffice365 == DateTime.MinValue || dataRenovacaoOffice365.Kind == DateTimeKind.Unspecified)
            dataRenovacaoOffice365 = agora.AddYears(1);
        else
            // Converter para UTC se não for
            dataRenovacaoOffice365 = dataRenovacaoOffice365.Kind == DateTimeKind.Utc 
                ? dataRenovacaoOffice365 
                : DateTime.SpecifyKind(dataRenovacaoOffice365.ToUniversalTime(), DateTimeKind.Utc);
        
        // Processar tickets para garantir que todas as datas sejam UTC
        var ticketsProcessados = request.Tickets.Select(ticket => 
        {
            var novoTicket = new Ticket
            {
                NumeroTicket = ticket.NumeroTicket,
                Resolucao = ticket.Resolucao,
                Solicitante = ticket.Solicitante,
                LocalSolicitante = ticket.LocalSolicitante,
                TipoAtendimento = ticket.TipoAtendimento,
                Analista = ticket.Analista,
                DescricaoCompleta = ticket.DescricaoCompleta,
                StatusTicket = ticket.StatusTicket,
                EmailSolicitante = ticket.EmailSolicitante,
                Assunto = ticket.Assunto,
                Setor = ticket.Setor,
                TempoSLA = ticket.TempoSLA,
                NomeOperador = ticket.NomeOperador
            };
            
            // Converter DataAbertura para UTC
            if (ticket.DataAbertura.Kind == DateTimeKind.Utc)
                novoTicket.DataAbertura = ticket.DataAbertura;
            else if (ticket.DataAbertura.Kind == DateTimeKind.Local)
                novoTicket.DataAbertura = ticket.DataAbertura.ToUniversalTime();
            else
                // Unspecified: assumir que já está em UTC e apenas marcar
                novoTicket.DataAbertura = DateTime.SpecifyKind(ticket.DataAbertura, DateTimeKind.Utc);
            
            // Converter DataFechamento para UTC
            if (ticket.DataFechamento.Kind == DateTimeKind.Utc)
                novoTicket.DataFechamento = ticket.DataFechamento;
            else if (ticket.DataFechamento.Kind == DateTimeKind.Local)
                novoTicket.DataFechamento = ticket.DataFechamento.ToUniversalTime();
            else
                // Unspecified: assumir que já está em UTC e apenas marcar
                novoTicket.DataFechamento = DateTime.SpecifyKind(ticket.DataFechamento, DateTimeKind.Utc);
            
            return novoTicket;
        }).ToList();
        
        var relatorio = new DadosRelatorio
        {
            HotelNome = request.HotelNome,
            DataCriacao = agora,
            DataGeracao = agora,
            QtdLicencasAntivirus = request.QtdLicencasAntivirus,
            QtdLicencasOffice365Standard = request.QtdLicencasOffice365Standard,
            QtdLicencasOffice365Basic = request.QtdLicencasOffice365Basic,
            StatusPreventivas = request.StatusPreventivas ?? "Em dia",
            DataRenovacaoAntivirus = dataRenovacaoAntivirus,
            DataRenovacaoOffice365 = dataRenovacaoOffice365,
            OutrosContratos = request.OutrosContratos,
            Tickets = ticketsProcessados,
            MelhoriasProximoMes = request.MelhoriasProximoMes ?? string.Empty,
            MelhoriasFeitas = request.MelhoriasFeitas ?? string.Empty,
            MelhoriasFeitasConfirmado = request.MelhoriasFeitasConfirmado,
            ResumoMes = request.ResumoMes ?? string.Empty,
            QtdMaquinasTotal = request.QtdMaquinasTotal,
            QtdMaquinasPreventivasFeitas = request.QtdMaquinasPreventivasFeitas,
            MotivoPreventivasIncompletas = request.MotivoPreventivasIncompletas ?? string.Empty,
            CriadoPorUsuarioId = request.CriadoPorUsuarioId,
            AssinaturaUsuarioNome = string.Empty,
            DataAssinatura = null,
            RelatorioAprovado = false
        };

        try
        {
            _context.Relatorios.Add(relatorio);
            await _context.SaveChangesAsync();

            return Ok(relatorio);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao salvar relatório: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            return StatusCode(500, new { message = "Erro ao salvar relatório", error = ex.Message });
        }
    }

    [HttpPut("{id}/melhorias")]
    public async Task<ActionResult> AtualizarMelhorias(int id, [FromBody] string melhorias)
    {
        if (melhorias == null)
            return BadRequest(new { message = "O campo melhorias é obrigatório" });

        var relatorio = await _context.Relatorios.FindAsync(id);
        if (relatorio == null)
            return NotFound();

        relatorio.MelhoriasProximoMes = melhorias ?? string.Empty;
        await _context.SaveChangesAsync();

        return Ok(relatorio);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DadosRelatorio>> AtualizarRelatorio(int id, [FromBody] RelatorioRequest request)
    {
        var relatorio = await _context.Relatorios.FindAsync(id);
        if (relatorio == null)
            return NotFound();

        // Verificar se o relatório foi assinado
        if (relatorio.RelatorioAprovado || relatorio.DataAssinatura.HasValue)
            return BadRequest(new { message = "Não é possível editar um relatório que já foi assinado" });

        if (string.IsNullOrWhiteSpace(request.HotelNome))
            return BadRequest(new { message = "Nome do hotel é obrigatório" });

        if (request.Tickets == null || request.Tickets.Count == 0)
            return BadRequest(new { message = "É necessário adicionar pelo menos um ticket" });

        var agora = DateTime.UtcNow;

        // Validar e processar datas de renovação - GARANTIR UTC
        DateTime dataRenovacaoAntivirus = request.DataRenovacaoAntivirus;
        DateTime dataRenovacaoOffice365 = request.DataRenovacaoOffice365;

        if (dataRenovacaoAntivirus == DateTime.MinValue || dataRenovacaoAntivirus.Kind == DateTimeKind.Unspecified)
            dataRenovacaoAntivirus = agora.AddYears(1);
        else
            dataRenovacaoAntivirus = dataRenovacaoAntivirus.Kind == DateTimeKind.Utc
                ? dataRenovacaoAntivirus
                : DateTime.SpecifyKind(dataRenovacaoAntivirus.ToUniversalTime(), DateTimeKind.Utc);

        if (dataRenovacaoOffice365 == DateTime.MinValue || dataRenovacaoOffice365.Kind == DateTimeKind.Unspecified)
            dataRenovacaoOffice365 = agora.AddYears(1);
        else
            dataRenovacaoOffice365 = dataRenovacaoOffice365.Kind == DateTimeKind.Utc
                ? dataRenovacaoOffice365
                : DateTime.SpecifyKind(dataRenovacaoOffice365.ToUniversalTime(), DateTimeKind.Utc);

        // Processar tickets para garantir que todas as datas sejam UTC
        var ticketsProcessados = request.Tickets.Select(ticket =>
        {
            var novoTicket = new Ticket
            {
                NumeroTicket = ticket.NumeroTicket,
                Resolucao = ticket.Resolucao,
                Solicitante = ticket.Solicitante,
                LocalSolicitante = ticket.LocalSolicitante,
                TipoAtendimento = ticket.TipoAtendimento,
                Analista = ticket.Analista,
                DescricaoCompleta = ticket.DescricaoCompleta,
                StatusTicket = ticket.StatusTicket,
                EmailSolicitante = ticket.EmailSolicitante,
                Assunto = ticket.Assunto,
                Setor = ticket.Setor,
                TempoSLA = ticket.TempoSLA,
                NomeOperador = ticket.NomeOperador
            };

            if (ticket.DataAbertura.Kind == DateTimeKind.Utc)
                novoTicket.DataAbertura = ticket.DataAbertura;
            else if (ticket.DataAbertura.Kind == DateTimeKind.Local)
                novoTicket.DataAbertura = ticket.DataAbertura.ToUniversalTime();
            else
                novoTicket.DataAbertura = DateTime.SpecifyKind(ticket.DataAbertura, DateTimeKind.Utc);

            if (ticket.DataFechamento.Kind == DateTimeKind.Utc)
                novoTicket.DataFechamento = ticket.DataFechamento;
            else if (ticket.DataFechamento.Kind == DateTimeKind.Local)
                novoTicket.DataFechamento = ticket.DataFechamento.ToUniversalTime();
            else
                novoTicket.DataFechamento = DateTime.SpecifyKind(ticket.DataFechamento, DateTimeKind.Utc);

            return novoTicket;
        }).ToList();

        // Atualizar campos do relatório
        relatorio.HotelNome = request.HotelNome;
        relatorio.DataGeracao = agora; // Atualizar data de geração
        relatorio.QtdLicencasAntivirus = request.QtdLicencasAntivirus;
        relatorio.QtdLicencasOffice365Standard = request.QtdLicencasOffice365Standard;
        relatorio.QtdLicencasOffice365Basic = request.QtdLicencasOffice365Basic;
        relatorio.StatusPreventivas = request.StatusPreventivas ?? "Em dia";
        relatorio.DataRenovacaoAntivirus = dataRenovacaoAntivirus;
        relatorio.DataRenovacaoOffice365 = dataRenovacaoOffice365;
        relatorio.OutrosContratos = request.OutrosContratos;
        relatorio.Tickets = ticketsProcessados;
        relatorio.MelhoriasProximoMes = request.MelhoriasProximoMes ?? string.Empty;
        relatorio.MelhoriasFeitas = request.MelhoriasFeitas ?? string.Empty;
        relatorio.ResumoMes = request.ResumoMes ?? string.Empty;
        relatorio.QtdMaquinasTotal = request.QtdMaquinasTotal;
        relatorio.QtdMaquinasPreventivasFeitas = request.QtdMaquinasPreventivasFeitas;
        relatorio.MotivoPreventivasIncompletas = request.MotivoPreventivasIncompletas ?? string.Empty;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(relatorio);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao atualizar relatório: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao atualizar relatório", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletarRelatorio(int id)
    {
        var relatorio = await _context.Relatorios.FindAsync(id);
        if (relatorio == null)
            return NotFound();

        _context.Relatorios.Remove(relatorio);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("hotel/{hotelNome}")]
    public async Task<ActionResult<List<DadosRelatorio>>> ObterRelatoriosPorHotel(string hotelNome)
    {
        var relatorios = await _context.Relatorios
            .Where(r => r.HotelNome == hotelNome)
            .OrderByDescending(r => r.DataGeracao)
            .ToListAsync();

        return Ok(relatorios);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DadosRelatorio>> ObterRelatorioPorId(int id)
    {
        var relatorio = await _context.Relatorios.FindAsync(id);
        
        if (relatorio == null)
            return NotFound();

        return Ok(relatorio);
    }

    [HttpGet]
    public async Task<ActionResult<List<string>>> ObterHoteis()
    {
        var hoteis = await _context.Relatorios
            .Select(r => r.HotelNome)
            .Distinct()
            .ToListAsync();

        return Ok(hoteis);
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> GerarPdf(int id)
    {
        var relatorio = await _context.Relatorios.FindAsync(id);
        
        if (relatorio == null)
            return NotFound();

        var estatisticas = ObterEstatisticasInterno(relatorio);
        var pdfBytes = await _pdfService.GerarPdf(relatorio, estatisticas);
        
        return File(pdfBytes, "application/pdf", $"Relatorio_{relatorio.HotelNome}_{relatorio.DataGeracao:yyyyMMdd}.pdf");
    }

    [HttpGet("{id}/estatisticas")]
    public async Task<ActionResult<EstatisticasRelatorio>> ObterEstatisticas(int id)
    {
        var relatorio = await _context.Relatorios.FindAsync(id);
        
        if (relatorio == null)
            return NotFound();

        var estatisticas = ObterEstatisticasInterno(relatorio);
        return Ok(estatisticas);
    }

    private EstatisticasRelatorio ObterEstatisticasInterno(DadosRelatorio relatorio)
    {
        return new EstatisticasRelatorio
        {
            ChamadosPorSolicitante = relatorio.Tickets
                .GroupBy(t => new { t.Solicitante, t.LocalSolicitante })
                .Select(g => new ChamadoPorSolicitante
                {
                    Solicitante = g.Key.Solicitante,
                    Local = g.Key.LocalSolicitante,
                    Quantidade = g.Count()
                })
                .OrderByDescending(x => x.Quantidade)
                .ToList(),

            ChamadosPorAnalista = relatorio.Tickets
                .GroupBy(t => t.Analista)
                .Select(g => new ChamadoPorAnalista
                {
                    Analista = g.Key,
                    Quantidade = g.Count()
                })
                .OrderByDescending(x => x.Quantidade)
                .ToList(),

            ChamadosPorTipo = relatorio.Tickets
                .GroupBy(t => t.TipoAtendimento)
                .Select(g => new ChamadoPorTipo
                {
                    Tipo = g.Key,
                    Quantidade = g.Count()
                })
                .OrderByDescending(x => x.Quantidade)
                .ToList(),

            ChamadosPorLocal = relatorio.Tickets
                .GroupBy(t => t.LocalSolicitante)
                .Select(g => new ChamadoPorLocal
                {
                    Local = g.Key,
                    Quantidade = g.Count()
                })
                .OrderByDescending(x => x.Quantidade)
                .ToList(),

            ChamadosPorAssunto = relatorio.Tickets
                .GroupBy(t => t.Assunto)
                .Select(g => new ChamadoPorAssunto
                {
                    Assunto = g.Key,
                    Quantidade = g.Count()
                })
                .OrderByDescending(x => x.Quantidade)
                .ToList(),

            TempoMedioResolucao = relatorio.Tickets.Any() 
                ? relatorio.Tickets.Select(t => (t.DataFechamento - t.DataAbertura).TotalHours).Average()
                : 0,

            TempoMedioSLA = relatorio.Tickets
                .Where(t => !string.IsNullOrEmpty(t.TempoSLA) && t.TempoSLA != "Não possui")
                .Select(t => ConverterTempoParaHoras(t.TempoSLA))
                .DefaultIfEmpty(0)
                .Average(),

            TotalComSLA = relatorio.Tickets.Count(t => !string.IsNullOrEmpty(t.TempoSLA) && t.TempoSLA != "Não possui"),

            TotalChamados = relatorio.Tickets.Count
        };
    }

    [HttpPost("{id}/assinar")]
    public async Task<ActionResult> AssinarRelatorio(int id, [FromBody] AssinaturaRequest request)
    {
        var relatorio = await _context.Relatorios.FindAsync(id);
        if (relatorio == null)
            return NotFound();

        relatorio.AssinaturaUsuarioNome = request.NomeUsuario ?? string.Empty;
        relatorio.DataAssinatura = DateTime.UtcNow;
        relatorio.RelatorioAprovado = request.Aprovado;

        await _context.SaveChangesAsync();
        return Ok(relatorio);
    }

    [HttpGet("filtros/aberturas")]
    public async Task<ActionResult> ObterRankingAberturas([FromQuery] string? hotelNome, [FromQuery] int? relatorioId)
    {
        DadosRelatorio? relatorio = null;
        if (relatorioId.HasValue)
        {
            relatorio = await _context.Relatorios.FindAsync(relatorioId.Value);
        }
        else if (!string.IsNullOrEmpty(hotelNome))
        {
            relatorio = await _context.Relatorios
                .Where(r => r.HotelNome == hotelNome)
                .OrderByDescending(r => r.DataGeracao)
                .FirstOrDefaultAsync();
        }

        if (relatorio == null)
            return NotFound();

        var ranking = relatorio.Tickets
            .GroupBy(t => t.Solicitante)
            .Select(g => new
            {
                Solicitante = g.Key,
                QuantidadeAbertas = g.Count(),
                Percentual = (double)g.Count() / relatorio.Tickets.Count * 100
            })
            .OrderByDescending(x => x.QuantidadeAbertas)
            .ToList();

        return Ok(ranking);
    }

    [HttpGet("filtros/fechamentos")]
    public async Task<ActionResult> ObterRankingFechamentos([FromQuery] string? hotelNome, [FromQuery] int? relatorioId)
    {
        DadosRelatorio? relatorio = null;
        if (relatorioId.HasValue)
        {
            relatorio = await _context.Relatorios.FindAsync(relatorioId.Value);
        }
        else if (!string.IsNullOrEmpty(hotelNome))
        {
            relatorio = await _context.Relatorios
                .Where(r => r.HotelNome == hotelNome)
                .OrderByDescending(r => r.DataGeracao)
                .FirstOrDefaultAsync();
        }

        if (relatorio == null)
            return NotFound();

        var ranking = relatorio.Tickets
            .Where(t => t.StatusTicket == "Finalizado")
            .GroupBy(t => t.Analista)
            .Select(g => new
            {
                Analista = g.Key,
                QuantidadeFechadas = g.Count(),
                Percentual = (double)g.Count() / relatorio.Tickets.Count(t => t.StatusTicket == "Finalizado") * 100
            })
            .OrderByDescending(x => x.QuantidadeFechadas)
            .ToList();

        return Ok(ranking);
    }

    [HttpGet("filtros/sla")]
    public async Task<ActionResult> ObterEstatisticasSLA([FromQuery] int relatorioId)
    {
        var relatorio = await _context.Relatorios.FindAsync(relatorioId);
        if (relatorio == null)
            return NotFound();

        var slaStats = relatorio.Tickets
            .Where(t => !string.IsNullOrEmpty(t.TempoSLA) && t.TempoSLA != "Não possui")
            .Select(t => new
            {
                Ticket = t.NumeroTicket,
                Analista = t.Analista,
                TempoSLA = t.TempoSLA,
                Solicitante = t.Solicitante,
                TempoGasto = ConverterTempoParaHoras(t.TempoSLA)
            })
            .OrderByDescending(x => x.TempoGasto)
            .ToList();

        var tempoMedioSLA = slaStats.Any() 
            ? slaStats.Average(x => x.TempoGasto) 
            : 0;

        return Ok(new
        {
            TempoMedioSLA = tempoMedioSLA,
            Detalhes = slaStats,
            TotalComSLA = slaStats.Count
        });
    }

    [HttpGet("comparar")]
    public async Task<ActionResult> CompararRelatorios([FromQuery] int relatorio1Id, [FromQuery] int relatorio2Id)
    {
        var rel1 = await _context.Relatorios.FindAsync(relatorio1Id);
        var rel2 = await _context.Relatorios.FindAsync(relatorio2Id);

        if (rel1 == null || rel2 == null)
            return NotFound();

        var comparacao = new
        {
            Relatorio1 = new
            {
                Id = rel1.Id,
                Hotel = rel1.HotelNome,
                Data = rel1.DataGeracao,
                TotalTickets = rel1.Tickets.Count,
                TempoMedio = rel1.Tickets.Any() 
                    ? rel1.Tickets.Select(t => (t.DataFechamento - t.DataAbertura).TotalHours).Average()
                    : 0
            },
            Relatorio2 = new
            {
                Id = rel2.Id,
                Hotel = rel2.HotelNome,
                Data = rel2.DataGeracao,
                TotalTickets = rel2.Tickets.Count,
                TempoMedio = rel2.Tickets.Any() 
                    ? rel2.Tickets.Select(t => (t.DataFechamento - t.DataAbertura).TotalHours).Average()
                    : 0
            },
            Diferenca = new
            {
                TotalTickets = rel2.Tickets.Count - rel1.Tickets.Count,
                TempoMedio = rel2.Tickets.Any() && rel1.Tickets.Any()
                    ? rel2.Tickets.Select(t => (t.DataFechamento - t.DataAbertura).TotalHours).Average() -
                      rel1.Tickets.Select(t => (t.DataFechamento - t.DataAbertura).TotalHours).Average()
                    : 0
            }
        };

        return Ok(comparacao);
    }

    [HttpGet("comparar-multiplos")]
    public async Task<ActionResult> CompararMultiplosRelatorios([FromQuery] string ids)
    {
        if (string.IsNullOrEmpty(ids))
            return BadRequest("IDs não fornecidos");

        var idsList = ids.Split(',').Select(id => int.TryParse(id.Trim(), out var parsedId) ? parsedId : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();

        if (idsList.Count < 2)
            return BadRequest("Forneça pelo menos 2 IDs de relatórios");

        var relatorios = await _context.Relatorios
            .Where(r => idsList.Contains(r.Id))
            .ToListAsync();

        if (relatorios.Count != idsList.Count)
            return NotFound("Alguns relatórios não foram encontrados");

        var relatoriosComparacao = relatorios.Select(rel => new
        {
            Id = rel.Id,
            Hotel = rel.HotelNome,
            Data = rel.DataGeracao,
            TotalTickets = rel.Tickets.Count,
            TempoMedio = rel.Tickets.Any()
                ? rel.Tickets.Where(t => t.DataFechamento > t.DataAbertura)
                    .Select(t => (t.DataFechamento - t.DataAbertura).TotalHours)
                    .DefaultIfEmpty(0)
                    .Average()
                : 0,
            TicketsFinalizados = rel.Tickets.Count(t => t.StatusTicket == "Finalizado"),
            TicketsPausados = rel.Tickets.Count(t => t.StatusTicket == "Pausado"),
            TicketsEmAndamento = rel.Tickets.Count(t => t.StatusTicket != "Finalizado" && t.StatusTicket != "Pausado")
        }).OrderBy(r => r.Data).ToList();

        var totalTickets = relatoriosComparacao.Sum(r => r.TotalTickets);
        var mediaTickets = relatoriosComparacao.Average(r => r.TotalTickets);
        var tempoMedioGeral = relatoriosComparacao
            .Where(r => r.TempoMedio > 0)
            .DefaultIfEmpty()
            .Average(r => r?.TempoMedio ?? 0);

        var comparacao = new
        {
            Relatorios = relatoriosComparacao,
            EstatisticasGerais = new
            {
                TotalTickets = totalTickets,
                MediaTickets = mediaTickets,
                TempoMedioGeral = tempoMedioGeral,
                TotalRelatorios = relatoriosComparacao.Count
            }
        };

        return Ok(comparacao);
    }

    private double ConverterTempoParaHoras(string tempoSLA)
    {
        // Formato esperado: "HH:MM" ou "Não possui"
        if (string.IsNullOrEmpty(tempoSLA) || tempoSLA == "Não possui")
            return 0;

        try
        {
            var partes = tempoSLA.Split(':');
            if (partes.Length == 2)
            {
                var horas = int.Parse(partes[0]);
                var minutos = int.Parse(partes[1]);
                return horas + (minutos / 60.0);
            }
        }
        catch { }

        return 0;
    }
}