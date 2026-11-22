namespace EF.Solutions.Business.Interfaces.Infraestructura;

/// <summary>
/// Autor: Elmer Fuentes.
/// Contrato definido en Business e implementado en Data para comprobar conectividad.
/// Relación: Web/Program.cs -> IDatabaseHealthService -> Data/MySqlDatabaseHealthService.cs.
/// </summary>
public interface IDatabaseHealthService
{
    /// <summary>
    /// Autor: Elmer Fuentes.
    /// Comprueba la conexión sin divulgar la cadena, el usuario ni el servidor.
    /// </summary>
    Task<bool> VerificarAsync(CancellationToken cancellationToken);
}
