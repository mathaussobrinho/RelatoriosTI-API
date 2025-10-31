namespace RelatoriosTI.API.Models;

public class RelatorioRequest
{
    public string HotelNome { get; set; } = string.Empty;
    public int QtdLicencasAntivirus { get; set; }
    public int QtdLicencasOffice365Standard { get; set; }
    public int QtdLicencasOffice365Basic { get; set; }
    public string StatusPreventivas { get; set; } = string.Empty;
    public DateTime DataRenovacaoAntivirus { get; set; }
    public DateTime DataRenovacaoOffice365 { get; set; }
    public string? OutrosContratos { get; set; }
    public List<Ticket> Tickets { get; set; } = new();
    public string MelhoriasProximoMes { get; set; } = string.Empty;
    public string MelhoriasFeitas { get; set; } = string.Empty;
    public bool MelhoriasFeitasConfirmado { get; set; } = false;
    public string ResumoMes { get; set; } = string.Empty;
    public int QtdMaquinasTotal { get; set; } = 0;
    public int QtdMaquinasPreventivasFeitas { get; set; } = 0;
    public string MotivoPreventivasIncompletas { get; set; } = string.Empty;
    public int CriadoPorUsuarioId { get; set; }
}