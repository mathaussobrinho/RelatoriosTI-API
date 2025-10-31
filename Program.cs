using Microsoft.EntityFrameworkCore;
using RelatoriosTI.API.Data;
using RelatoriosTI.API.Services;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configurar porta local
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://localhost:{port}");

QuestPDF.Settings.License = LicenseType.Community;

// Controllers, Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ========= 🔐 CORS ==========
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // Permitir localhost e 127.0.0.1 para desenvolvimento local
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "http://127.0.0.1:3000",
                "http://127.0.0.1:5173"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
    });
});

// ========= 💾 Banco de Dados ==========
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=RelatoriosTI.db";

Console.WriteLine("Configurando banco de dados SQLite...");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<PdfService>();

var app = builder.Build();

Console.WriteLine("Aplicação iniciada!");

// ========= 🧪 Swagger ==========
// Swagger sempre habilitado para facilitar testes locais
app.UseSwagger();
app.UseSwaggerUI();

// ========= 🌐 CORS ==========
// IMPORTANTE: CORS deve vir ANTES de UseAuthorization e MapControllers
app.UseCors("AllowFrontend");

// ========= 🚀 Pipeline ==========
/*
 * Desabilitei temporariamente para evitar conflitos de redirect no preflight CORS.
 * Depois de tudo funcionando, você pode reativar se quiser.
 */
// app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

// ========= 🧠 Aplicar migrations ==========
Console.WriteLine("Aplicando migrations...");
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Console.WriteLine("Migrations aplicadas com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao aplicar migrations: {ex.Message}");
    }
}

// Health check
app.MapGet("/", () => "API está rodando! Acesse /swagger");
app.MapGet("/health", () => "OK");

Console.WriteLine($"Servidor rodando na porta {port}");
app.Run();
