namespace RelatoriosTI.API.Models;

public class DadosRelatorio
{
    public int Id { get; set; }
    public string HotelNome { get; set; } = string.Empty;
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
    public int CriadoPorUsuarioId { get; set; }
}