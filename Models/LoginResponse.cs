namespace RelatoriosTI.API.Models;

public class LoginResponse
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool EhAdministrador { get; set; }
    public List<string> HoteisPermitidos { get; set; } = new();
}