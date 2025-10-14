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
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.Senha == request.Senha);

        if (usuario == null)
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
        var existe = await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email);
        if (existe)
            return BadRequest(new { message = "Email já cadastrado" });

        usuario.DataCriacao = DateTime.Now;
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return Ok(usuario);
    }

    [HttpGet("usuarios")]
    public async Task<ActionResult<List<Usuario>>> ObterUsuarios()
    {
        var usuarios = await _context.Usuarios.ToListAsync();
        return Ok(usuarios);
    }

    [HttpPut("usuarios/{id}")]
    public async Task<ActionResult> AtualizarUsuario(int id, [FromBody] Usuario usuario)
    {
        var usuarioExistente = await _context.Usuarios.FindAsync(id);
        if (usuarioExistente == null)
            return NotFound();

        usuarioExistente.Email = usuario.Email;
        usuarioExistente.Senha = usuario.Senha;
        usuarioExistente.EhAdministrador = usuario.EhAdministrador;
        usuarioExistente.HoteisPermitidos = usuario.HoteisPermitidos;

        await _context.SaveChangesAsync();
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