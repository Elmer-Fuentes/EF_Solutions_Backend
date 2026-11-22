using EF.Solutions.Business.Configuracion;
using EF.Solutions.Business.Interfaces.Autenticacion;
using MySqlConnector;

namespace EF.Solutions.Data.Repositories.Autenticacion;

/// <summary>Autor: Elmer Fuentes. Persiste el alta en las tres tablas MySQL de identidad y tenants.</summary>
public sealed class RegistroMySqlRepositorio(DatabaseSettings settings) : IRegistroRepositorio
{
    #region Crear identidad y espacio
    /// <inheritdoc />
    public async Task<(ulong Usuario, ulong Tenant)?> CrearAsync(
        string nombre, string correo, string hash, CancellationToken ct)
    {
        var cadena = new MySqlConnectionStringBuilder
        {
            Server = settings.Host,
            Port = settings.Port,
            Database = settings.Database,
            UserID = settings.Username,
            Password = settings.Password,
            SslMode = Enum.Parse<MySqlSslMode>(settings.SslMode, ignoreCase: true),
            PersistSecurityInfo = false,
            ConnectionTimeout = 5
        };
        if (!string.IsNullOrWhiteSpace(settings.SslCa))
            cadena.SslCa = settings.SslCa;

        await using var conexion = new MySqlConnection(cadena.ConnectionString);
        await conexion.OpenAsync(ct);
        await using var transaccion = await conexion.BeginTransactionAsync(ct);
        await using var usuario = new MySqlCommand("""
            INSERT INTO ef_seg_usuario (Email, PasswordHash, Nombre, FechaCreacionUtc)
            VALUES (@correo, @hash, @nombre, UTC_TIMESTAMP(6))
            """, conexion, transaccion);
        usuario.Parameters.AddWithValue("@correo", correo);
        usuario.Parameters.AddWithValue("@hash", hash);
        usuario.Parameters.AddWithValue("@nombre", nombre);
        try
        {
            await usuario.ExecuteNonQueryAsync(ct);
        }
        catch (MySqlException error) when (error.Number == 1062)
        {
            return null;
        }

        await using var tenant = new MySqlCommand("""
            INSERT INTO ef_ten_tenant (Nombre, FechaCreacionUtc)
            VALUES (@nombre, UTC_TIMESTAMP(6))
            """, conexion, transaccion);
        tenant.Parameters.AddWithValue("@nombre", nombre);
        await tenant.ExecuteNonQueryAsync(ct);

        await using var membresia = new MySqlCommand("""
            INSERT INTO ef_ten_membresia (TenantId, IdUsuario, RolBase, Estado, FechaCreacionUtc)
            VALUES (@tenant, @usuario, 'OWNER', 'ACTIVO', UTC_TIMESTAMP(6))
            """, conexion, transaccion);
        membresia.Parameters.AddWithValue("@tenant", tenant.LastInsertedId);
        membresia.Parameters.AddWithValue("@usuario", usuario.LastInsertedId);
        await membresia.ExecuteNonQueryAsync(ct);

        await transaccion.CommitAsync(ct);
        return ((ulong)usuario.LastInsertedId, (ulong)tenant.LastInsertedId);
    }
    #endregion
}
