namespace RelatoriosTI.API.Models;

public class EstatisticasRelatorio
{
    public List<ChamadoPorSolicitante> ChamadosPorSolicitante { get; set; } = new();
    public List<ChamadoPorAnalista> ChamadosPorAnalista { get; set; } = new();
    public List<ChamadoPorTipo> ChamadosPorTipo { get; set; } = new();
    public List<ChamadoPorLocal> ChamadosPorLocal { get; set; } = new();
    public double TempoMedioResolucao { get; set; }
    public int TotalChamados { get; set; }
}

public class ChamadoPorSolicitante
{
    public string Solicitante { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}

public class ChamadoPorAnalista
{
    public string Analista { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}

public class ChamadoPorTipo
{
    public string Tipo { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}

public class ChamadoPorLocal
{
    public string Local { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}