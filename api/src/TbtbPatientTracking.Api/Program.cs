using Microsoft.EntityFrameworkCore;
using TbtbPatientTracking.Application;
using TbtbPatientTracking.Infrastructure;
using TbtbPatientTracking.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// La cadena de conexión vive en appsettings.Development.json (ignorado por git).
// El repositorio incluye appsettings.Example.json como referencia.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Falta la cadena de conexión 'DefaultConnection'. Copie appsettings.Example.json a appsettings.Development.json.");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS: los orígenes que pueden llamar al API vienen de la configuración (Cors:AllowedOrigins).
// En desarrollo Angular usa además un proxy, pero el API debe declarar por sí mismo quién puede consumirlo.
const string CorsPolicyName = "AllowedOrigins";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy(CorsPolicyName, policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // En desarrollo se aplican las migraciones al iniciar, para que el API arranque con la base lista.
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicyName);
app.UseAuthorization();
app.MapControllers();

app.Run();
