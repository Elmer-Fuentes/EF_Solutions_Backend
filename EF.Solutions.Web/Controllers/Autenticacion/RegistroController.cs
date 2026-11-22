using System.Globalization;
using EF.Solutions.Business.Services.Autenticacion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EF.Solutions.Web.Controllers.Autenticacion;

/// <summary>Autor: Elmer Fuentes. Expone el alta pública mediante RegistroService.</summary>
[ApiController]
[Route("api/v1/autenticacion/registro")]
public sealed class RegistroController(RegistroService servicio) : ControllerBase
{
    #region Registrar
    /// <summary>Autor: Elmer Fuentes. Crea una identidad y su propio espacio.</summary>
    /// <returns>201 con identificadores, 400 si los datos no son válidos o 409 ante duplicidad.</returns>
    [HttpPost]
    [EnableRateLimiting("registro")]
    public async Task<IActionResult> RegistrarAsync(RegistroSolicitud solicitud, CancellationToken ct)
    {
        var (error, alta) = await servicio.RegistrarAsync(solicitud.Nombre, solicitud.Correo, solicitud.Clave, ct);
        if (error is not null)
            return BadRequest(new { mensaje = error });
        if (alta is null)
            return Conflict(new { mensaje = "No se pudo registrar ese correo." });

        return StatusCode(StatusCodes.Status201Created, new
        {
            usuarioId = alta.Value.Usuario.ToString(CultureInfo.InvariantCulture),
            tenantId = alta.Value.Tenant.ToString(CultureInfo.InvariantCulture)
        });
    }
    #endregion
}

/// <summary>Autor: Elmer Fuentes. Datos de entrada del alta mediante RegistroService.</summary>
public sealed record RegistroSolicitud(string? Nombre, string? Correo, string? Clave);
