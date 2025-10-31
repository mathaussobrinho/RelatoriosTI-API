namespace RelatoriosTI.API.Models;

public class DadosRelatorio
{
    public int Id { get; set; }
    public string HotelNome { get; set; } = string.Empty;
    
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime DataGeracao { get; set; }
    
    public int QtdLicencasAntivirus { get; set; }
    public int QtdLicencasOffice365Standard { get; set; }
    public int QtdLicencasOffice365Basic { get; set; }
    public string StatusPreventivas { get; set; } = string.Empty;
    
    public DateTime DataRenovacaoAntivirus { get; set; }
    public DateTime DataRenovacaoOffice365 { get; set; }
    
    public string? OutrosContratos { get; set; }
    public List<Ticket> Tickets { get; set; } = new();
    public string? CaminhoArquivoPdf { get; set; }
    public string MelhoriasProximoMes { get; set; } = string.Empty;
    public string MelhoriasFeitas { get; set; } = string.Empty; // Melhorias que foram feitas
    public bool MelhoriasFeitasConfirmado { get; set; } = false; // Se as melhorias foram confirmadas como feitas
    public string ResumoMes { get; set; } = string.Empty; // Resumo de tudo que foi feito no mês
    
    // Preventivas detalhadas
    public int QtdMaquinasTotal { get; set; } = 0;
    public int QtdMaquinasPreventivasFeitas { get; set; } = 0;
    public string MotivoPreventivasIncompletas { get; set; } = string.Empty;
    
    // Assinatura/OK do relatório
    public string AssinaturaUsuarioNome { get; set; } = string.Empty;
    public DateTime? DataAssinatura { get; set; }
    public bool RelatorioAprovado { get; set; } = false;
    
    public int CriadoPorUsuarioId { get; set; }
    public Usuario? CriadoPor { get; set; } // Propriedade de navegação para EF Core
}