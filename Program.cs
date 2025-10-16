using Microsoft.EntityFrameworkCore;
using RelatoriosTI.API.Data;
using RelatoriosTI.API.Services;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configurar porta (Railway)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

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
        policy.WithOrigins("https://funipro.shop")
              .AllowAnyMethod()
              .AllowAnyHeader();
              // Se usar cookies: .AllowCredentials();
    });
});

// ========= 💾 Banco de Dados ==========
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine("Configurando banco de dados...");

if (connectionString?.StartsWith("postgresql://") == true)
{
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<PdfService>();

var app = builder.Build();

Console.WriteLine("Aplicação iniciada!");

// ========= 🌐 CORS ==========
app.UseCors("AllowFrontend");

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

// ========= 🧪 Swagger ==========
if (app.Environment.IsDevelopment() || true) // deixa sempre ligado
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ========= 🚀 Pipeline ==========
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Health check
app.MapGet("/", () => "API está rodando! Acesse /swagger");
app.MapGet("/health", () => "OK");

Console.WriteLine($"Servidor rodando na porta {port}");
app.Run();
