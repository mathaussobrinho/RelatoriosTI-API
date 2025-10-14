namespace RelatoriosTI.API.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public bool EhAdministrador { get; set; }
    public List<string> HoteisPermitidos { get; set; } = new();
    public DateTime DataCriacao { get; set; } = DateTime.Now;
}