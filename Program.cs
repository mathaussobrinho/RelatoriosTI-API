var builder = WebApplication.CreateBuilder(args);

// Adicionar política de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("https://funipro.shop") // domínio do seu frontend
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// ... outros serviços
builder.Services.AddControllers();

var app = builder.Build();

// Usar CORS
app.UseCors("AllowFrontend");

// ... pipeline padrão
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
