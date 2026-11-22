namespace EF.Solutions.Business.Interfaces.Autenticacion;

/// <summary>Autor: Elmer Fuentes. Contrato implementado por RegistroMySqlRepositorio.</summary>
public interface IRegistroRepositorio
{
    #region Crear identidad y espacio
    /// <summary>Autor: Elmer Fuentes. Guarda identidad, espacio y membresía de forma atómica.</summary>
    /// <returns>Identificadores nuevos o null si el correo ya existe.</returns>
    Task<(ulong Usuario, ulong Tenant)?> CrearAsync(string nombre, string correo, string hash, CancellationToken ct);
    #endregion
}
