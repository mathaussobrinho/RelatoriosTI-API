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
        var relatorio = new DadosRelatorio
        {
            HotelNome = request.HotelNome,
            DataGeracao = DateTime.Now,
            QtdLicencasAntivirus = request.QtdLicencasAntivirus,
            QtdLicencasOffice365Standard = request.QtdLicencasOffice365Standard,
            QtdLicencasOffice365Basic = request.QtdLicencasOffice365Basic,
            StatusPreventivas = request.StatusPreventivas,
            DataRenovacaoAntivirus = request.DataRenovacaoAntivirus,
            DataRenovacaoOffice365 = request.DataRenovacaoOffice365,
            OutrosContratos = request.OutrosContratos,
            Tickets = request.Tickets,
            MelhoriasProximoMes = request.MelhoriasProximoMes,
            CriadoPorUsuarioId = request.CriadoPorUsuarioId
        };

        _context.Relatorios.Add(relatorio);
        await _context.SaveChangesAsync();

        return Ok(relatorio);
    }

    [HttpPut("{id}/melhorias")]
    public async Task<ActionResult> AtualizarMelhorias(int id, [FromBody] string melhorias)
    {
        var relatorio = await _context.Relatorios.FindAsync(id);
        if (relatorio == null)
            return NotFound();

        relatorio.MelhoriasProximoMes = melhorias;
        await _context.SaveChangesAsync();

        return Ok(relatorio);
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

            TempoMedioResolucao = relatorio.Tickets.Any() 
                ? relatorio.Tickets.Select(t => (t.DataFechamento - t.DataAbertura).TotalHours).Average()
                : 0,

            TotalChamados = relatorio.Tickets.Count
        };
    }
}