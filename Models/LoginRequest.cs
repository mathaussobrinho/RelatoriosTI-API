namespace RelatoriosTI.API.Models;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}