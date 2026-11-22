using EF.Solutions.Business.Configuracion;
using EF.Solutions.Business.Interfaces.Infraestructura;
using EF.Solutions.Data.Connections.MySql;
using EF.Solutions.Business.Interfaces.Autenticacion;
using EF.Solutions.Business.Services.Autenticacion;
using EF.Solutions.Data.Repositories.Autenticacion;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

// Autor: Elmer Fuentes.
// Arranque de API HTTP. Web configura dependencias; Business define contratos y Data los implementa.
// .env es exclusivo de desarrollo local. En Railway se usan variables protegidas del servicio.
var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase))
{
    var customPath = Environment.GetEnvironmentVariable("EF_CONFIG_FILE");
    var envPath = string.IsNullOrWhiteSpace(customPath)
        ? Path.Combine(Directory.GetCurrentDirectory(), ".env")
        : Path.GetFullPath(customPath);
    if (!File.Exists(envPath) && !string.IsNullOrWhiteSpace(customPath))
        throw new FileNotFoundException("No se encontró el archivo de configuración EF_CONFIG_FILE.");
    if (File.Exists(envPath)) DotNetEnv.Env.NoClobber().Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("registro", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
});
builder.Services.AddScoped<IRegistroRepositorio, RegistroMySqlRepositorio>();
builder.Services.AddScoped<RegistroService>();

#region Configuración de base de datos
var cfg = builder.Configuration;
string Required(string key) => !string.IsNullOrWhiteSpace(cfg[key])
    ? cfg[key]!
    : throw new InvalidOperationException($"Falta la configuración obligatoria: {key}");

var provider = Required("EF_DATABASE:PROVIDER");
if (!provider.Equals("MySql", StringComparison.OrdinalIgnoreCase))
    throw new NotSupportedException("Este arranque solo implementa MySQL; otros motores requerirán adaptadores probados.");

var host = Required("EF_DATABASE:HOST");
var ssl = Required("EF_DATABASE:SSL_MODE");
var localServer = host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
    || host is "127.0.0.1" or "::1";
if ((!builder.Environment.IsDevelopment() || !localServer)
    && !ssl.Equals("VerifyFull", StringComparison.OrdinalIgnoreCase))
    throw new InvalidOperationException("Servidores remotos o producción requieren SSL_MODE=VerifyFull.");

if (!uint.TryParse(Required("EF_DATABASE:PORT"), out var port) || port is 0 or > 65535)
    throw new InvalidOperationException("Puerto de base de datos inválido.");

var options = new DatabaseSettings(
    provider, host, port, Required("EF_DATABASE:NAME"),
    Required("EF_DATABASE:USERNAME"), Required("EF_DATABASE:PASSWORD"),
    ssl, cfg["EF_DATABASE:SSL_CA"]);
builder.Services.AddSingleton(options);
builder.Services.AddScoped<IDatabaseHealthService, MySqlDatabaseHealthService>();
#endregion

var app = builder.Build();
app.UseRateLimiter();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Diagnóstico de DB accesible SOLO en desarrollo; nunca debe mostrar secretos.
if (app.Environment.IsDevelopment())
{
    app.MapGet("/health/database", async (IDatabaseHealthService service, CancellationToken ct) =>
    {
        try { return await service.VerificarAsync(ct) ? Results.Ok(new { status = "ok" }) : Results.StatusCode(503); }
        catch { return Results.StatusCode(503); }
    });
}
app.Run();
