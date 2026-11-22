using EF.Solutions.Business.Configuracion;
using EF.Solutions.Business.Interfaces.Infraestructura;
using EF.Solutions.Data.Connections.MySql;
using Microsoft.OpenApi;

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
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "EF Solutions API", Version = "v1" }));
}

#region Configuración de base de datos
var cfg = builder.Configuration;
#region Leer configuración obligatoria
/// <summary>Autor: Elmer Fuentes. Lee una clave obligatoria para configurar la API.</summary>
/// <param name="key">Clave de configuración de ASP.NET Core.</param>
/// <returns>Valor configurado.</returns>
/// <exception cref="InvalidOperationException">La clave está ausente o vacía.</exception>
string Required(string key) => !string.IsNullOrWhiteSpace(cfg[key])
    ? cfg[key]!
    : throw new InvalidOperationException($"Falta la configuración obligatoria: {key}");
#endregion

var provider = Required("EF_DATABASE:PROVIDER");
if (!provider.Equals("MySql", StringComparison.OrdinalIgnoreCase))
    throw new NotSupportedException("Este arranque solo implementa MySQL; otros motores requerirán adaptadores probados.");

var host = Required("EF_DATABASE:HOST");
var username = Required("EF_DATABASE:USERNAME");
if (username.Equals("root", StringComparison.OrdinalIgnoreCase))
    throw new InvalidOperationException("La API requiere una cuenta MySQL de aplicación.");

var ssl = Required("EF_DATABASE:SSL_MODE");
if (!new[] { "None", "Preferred", "Required", "VerifyCA", "VerifyFull" }.Contains(ssl, StringComparer.OrdinalIgnoreCase))
    throw new InvalidOperationException("Modo TLS de base de datos inválido.");
var localServer = host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
    || host is "127.0.0.1" or "::1";
if ((!builder.Environment.IsDevelopment() || !localServer)
    && !ssl.Equals("VerifyFull", StringComparison.OrdinalIgnoreCase))
    throw new InvalidOperationException("Servidores remotos o producción requieren SSL_MODE=VerifyFull.");

if (!uint.TryParse(Required("EF_DATABASE:PORT"), out var port) || port is 0 or > 65535)
    throw new InvalidOperationException("Puerto de base de datos inválido.");

var options = new DatabaseSettings(
    provider, host, port, Required("EF_DATABASE:NAME"),
    username, Required("EF_DATABASE:PASSWORD"),
    ssl, cfg["EF_DATABASE:SSL_CA"]);
builder.Services.AddSingleton(options);
builder.Services.AddScoped<IDatabaseHealthService, MySqlDatabaseHealthService>();
#endregion

var app = builder.Build();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Diagnóstico de DB accesible SOLO en desarrollo; nunca debe mostrar secretos.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("./v1/swagger.json", "EF Solutions API v1"));

    app.MapGet("/health/database", async (IDatabaseHealthService service, CancellationToken ct) =>
    {
        return await service.VerificarAsync(ct) ? Results.Ok(new { status = "ok" }) : Results.StatusCode(503);
    }).Produces(StatusCodes.Status200OK).Produces(StatusCodes.Status503ServiceUnavailable);
}
app.Run();
