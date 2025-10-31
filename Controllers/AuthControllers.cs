using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RelatoriosTI.API.Data;
using RelatoriosTI.API.Models;

namespace RelatoriosTI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
            return BadRequest(new { message = "Email e senha são obrigatórios" });

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.Senha))
            return Unauthorized(new { message = "Email ou senha incorretos" });

        return Ok(new LoginResponse
        {
            Id = usuario.Id,
            Email = usuario.Email,
            EhAdministrador = usuario.EhAdministrador,
            HoteisPermitidos = usuario.HoteisPermitidos
        });
    }

    [HttpPost("usuarios")]
    public async Task<ActionResult<Usuario>> CriarUsuario([FromBody] Usuario usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario.Email))
            return BadRequest(new { message = "Email é obrigatório" });

        if (string.IsNullOrWhiteSpace(usuario.Senha) || usuario.Senha.Length < 6)
            return BadRequest(new { message = "Senha deve ter no mínimo 6 caracteres" });

        var existe = await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email);
        if (existe)
            return BadRequest(new { message = "Email já cadastrado" });

        // Hash da senha antes de salvar
        usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuario.Senha);
        usuario.DataCriacao = DateTime.UtcNow;
        
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        // Não retornar a senha (mesmo que seja hash)
        usuario.Senha = string.Empty;
        return Ok(usuario);
    }

    [HttpGet("usuarios")]
    public async Task<ActionResult<List<Usuario>>> ObterUsuarios()
    {
        var usuarios = await _context.Usuarios.ToListAsync();
        
        // Remover senhas da resposta
        foreach (var usuario in usuarios)
        {
            usuario.Senha = string.Empty;
        }
        
        return Ok(usuarios);
    }

    [HttpPut("usuarios/{id}")]
    public async Task<ActionResult> AtualizarUsuario(int id, [FromBody] Usuario usuario)
    {
        var usuarioExistente = await _context.Usuarios.FindAsync(id);
        if (usuarioExistente == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(usuario.Email))
            return BadRequest(new { message = "Email é obrigatório" });

        // Verificar se outro usuário já usa esse email
        var emailEmUso = await _context.Usuarios
            .AnyAsync(u => u.Email == usuario.Email && u.Id != id);
        if (emailEmUso)
            return BadRequest(new { message = "Email já está em uso por outro usuário" });

        usuarioExistente.Email = usuario.Email;
        
        // Se uma nova senha foi fornecida, fazer hash
        if (!string.IsNullOrWhiteSpace(usuario.Senha))
        {
            if (usuario.Senha.Length < 6)
                return BadRequest(new { message = "Senha deve ter no mínimo 6 caracteres" });
            
            usuarioExistente.Senha = BCrypt.Net.BCrypt.HashPassword(usuario.Senha);
        }
        
        usuarioExistente.EhAdministrador = usuario.EhAdministrador;
        usuarioExistente.HoteisPermitidos = usuario.HoteisPermitidos;

        await _context.SaveChangesAsync();
        
        // Não retornar a senha
        usuarioExistente.Senha = string.Empty;
        return Ok(usuarioExistente);
    }

    [HttpDelete("usuarios/{id}")]
    public async Task<ActionResult> DeletarUsuario(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
            return NotFound();

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();
        return Ok();
    }
}