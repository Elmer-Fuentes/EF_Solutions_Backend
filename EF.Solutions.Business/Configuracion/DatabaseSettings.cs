namespace EF.Solutions.Business.Configuracion;

/// <summary>
/// Autor: Elmer Fuentes.
/// Configuración de conexión resuelta por la capa Web desde variables de entorno.
/// No se serializa en respuestas API ni se registra en logs.
/// </summary>
public sealed record DatabaseSettings(
    string Provider,
    string Host,
    uint Port,
    string Database,
    string Username,
    string Password,
    string SslMode,
    string? SslCa);
