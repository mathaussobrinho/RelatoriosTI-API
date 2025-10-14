namespace RelatoriosTI.API.Models;

public class Ticket
{
    public string NumeroTicket { get; set; } = string.Empty;
    public string Resolucao { get; set; } = string.Empty;
    public DateTime DataAbertura { get; set; }
    public DateTime DataFechamento { get; set; }
    public string Solicitante { get; set; } = string.Empty;
    public string LocalSolicitante { get; set; } = string.Empty;
    public string TipoAtendimento { get; set; } = string.Empty;
    public string Analista { get; set; } = string.Empty;
    public string DescricaoCompleta { get; set; } = string.Empty;
    public string StatusTicket { get; set; } = string.Empty;
    public string EmailSolicitante { get; set; } = string.Empty;
    public string Assunto { get; set; } = string.Empty;
}