using Microsoft.EntityFrameworkCore;
using RelatoriosTI.API.Models;
using System.Text.Json;

namespace RelatoriosTI.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DadosRelatorio> Relatorios { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DadosRelatorio>()
            .Property(r => r.Tickets)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<Ticket>>(v, (JsonSerializerOptions?)null) ?? new List<Ticket>()
            );

        modelBuilder.Entity<Usuario>()
            .Property(u => u.HoteisPermitidos)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
            );

        // Criar usuário administrador padrão
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                Email = "mathaus@admin",
                Senha = "mathaus@123",
                EhAdministrador = true,
                DataCriacao = DateTime.Now
            }
        );
    }
}