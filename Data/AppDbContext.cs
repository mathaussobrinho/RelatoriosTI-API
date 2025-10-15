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
        // Configura o schema para PostgreSQL
        modelBuilder.HasDefaultSchema("public");

        // Configura relacionamento entre DadosRelatorio e Usuario
        modelBuilder.Entity<DadosRelatorio>()
            .HasOne(d => d.CriadoPor)
            .WithMany()
            .HasForeignKey(d => d.CriadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configura conversão de JSON para Tickets
        modelBuilder.Entity<DadosRelatorio>()
            .Property(r => r.Tickets)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }),
                v => JsonSerializer.Deserialize<List<Ticket>>(v, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Ticket>()
            );

        // Configura conversão de JSON para HoteisPermitidos
        modelBuilder.Entity<Usuario>()
            .Property(u => u.HoteisPermitidos)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }),
                v => JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<string>()
            );

        // Criar usuário administrador padrão
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                Email = "mathaus@admin",
                Senha = BCrypt.Net.BCrypt.HashPassword("mathaus@123"),
                EhAdministrador = true,
                DataCriacao = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                HoteisPermitidos = new List<string>() // Lista vazia pois é admin
            }
        );

        // Índices para performance
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<DadosRelatorio>()
            .HasIndex(r => r.DataCriacao);

        modelBuilder.Entity<DadosRelatorio>()
            .HasIndex(r => r.HotelNome);
    }
}