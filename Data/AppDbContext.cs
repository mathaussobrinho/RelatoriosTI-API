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
        // Configura relacionamento entre DadosRelatorio e Usuario
        modelBuilder.Entity<DadosRelatorio>()
            .HasOne(d => d.CriadoPor)
            .WithMany()
            .HasForeignKey(d => d.CriadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configura conversão de JSON para Tickets (SQLite armazena como TEXT)
        modelBuilder.Entity<DadosRelatorio>()
            .Property(r => r.Tickets)
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }),
                v => JsonSerializer.Deserialize<List<Ticket>>(v, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Ticket>()
            )
            .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<Ticket>>(
                (c1, c2) => c1 == c2,
                c => c.GetHashCode(),
                c => c.ToList()));

        // Configura conversão de JSON para HoteisPermitidos (SQLite armazena como TEXT)
        modelBuilder.Entity<Usuario>()
            .Property(u => u.HoteisPermitidos)
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }),
                v => JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<string>()
            )
            .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                (c1, c2) => c1 == c2,
                c => c.GetHashCode(),
                c => c.ToList()));

        // Criar usuário administrador padrão
        // Hash estático para evitar warning de mudanças dinâmicas
        // Senha: mathaus@123
        var senhaHash = "$2a$11$v2YsLtwHrb72xNnvQ4gzoOY527yzeM3XcOSRS.L018TXSHTUjTIGa";
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                Email = "mathaus@admin",
                Senha = senhaHash,
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