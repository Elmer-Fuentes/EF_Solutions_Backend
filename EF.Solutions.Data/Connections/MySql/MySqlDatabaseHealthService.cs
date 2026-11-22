using EF.Solutions.Business.Configuracion;
using EF.Solutions.Business.Interfaces.Infraestructura;
using MySqlConnector;

namespace EF.Solutions.Data.Connections.MySql;

/// <summary>
/// Autor: Elmer Fuentes.
/// Servicio de diagnóstico de MySQL; satisface el contrato de Business.
/// Relación: Web/Program.cs -> Business/IDatabaseHealthService.cs -> esta clase.
/// </summary>
public sealed class MySqlDatabaseHealthService : IDatabaseHealthService
{
    private readonly DatabaseSettings _settings;

    /// <summary>
    /// Autor: Elmer Fuentes.
    /// Inicializa el diagnóstico con parámetros obtenidos del entorno del servidor.
    /// </summary>
    public MySqlDatabaseHealthService(DatabaseSettings settings) => _settings = settings;

    #region Diagnóstico de conexión
    /// <inheritdoc />
    public async Task<bool> VerificarAsync(CancellationToken cancellationToken)
    {
        var ssl = Enum.Parse<MySqlSslMode>(_settings.SslMode, ignoreCase: true);
        var connString = new MySqlConnectionStringBuilder
        {
            Server = _settings.Host,
            Port = _settings.Port,
            Database = _settings.Database,
            UserID = _settings.Username,
            Password = _settings.Password,
            SslMode = ssl,
            PersistSecurityInfo = false,
            ConnectionTimeout = 5
        };
        if (!string.IsNullOrWhiteSpace(_settings.SslCa))
            connString.SslCa = _settings.SslCa;
        await using var connection = new MySqlConnection(connString.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand("SELECT 1", connection);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result) == 1;
    }
    #endregion
}
