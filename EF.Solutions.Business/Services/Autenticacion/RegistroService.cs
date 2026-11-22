using System.Net.Mail;
using System.Security.Cryptography;
using EF.Solutions.Business.Interfaces.Autenticacion;

namespace EF.Solutions.Business.Services.Autenticacion;

/// <summary>Autor: Elmer Fuentes. Valida el alta y aplica hashing antes de llamar a IRegistroRepositorio.</summary>
public sealed class RegistroService(IRegistroRepositorio repositorio)
{
    #region Registrar identidad
    /// <summary>Autor: Elmer Fuentes. Prepara el registro y delega la escritura al repositorio.</summary>
    /// <returns>Error de validación, o resultado del repositorio para los datos válidos.</returns>
    public async Task<(string? Error, (ulong Usuario, ulong Tenant)? Alta)> RegistrarAsync(
        string? nombre, string? correo, string? clave, CancellationToken ct)
    {
        nombre = nombre?.Trim();
        correo = correo?.Trim().ToLowerInvariant();
        if (nombre is null || nombre.Length is < 1 or > 160)
            return ("El nombre debe contener entre 1 y 160 caracteres.", null);
        if (correo is null || correo.Length > 254 || !MailAddress.TryCreate(correo, out var direccion)
            || !string.Equals(direccion.Address, correo, StringComparison.Ordinal))
            return ("Ingresa un correo válido.", null);
        if (clave is null || clave.Length is < 12 or > 128)
            return ("La contraseña debe contener entre 12 y 128 caracteres.", null);

        ct.ThrowIfCancellationRequested();
        const int iteraciones = 600_000;
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(clave, salt, iteraciones, HashAlgorithmName.SHA256, 32);
        var credencial = $"pbkdf2-sha256${iteraciones}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        return (null, await repositorio.CrearAsync(nombre, correo, credencial, ct));
    }
    #endregion
}
