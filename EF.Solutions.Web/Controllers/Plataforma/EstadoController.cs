using Microsoft.AspNetCore.Mvc;

namespace EF.Solutions.Web.Controllers.Plataforma;

/// <summary>
/// Autor: Elmer Fuentes.
/// Expone estado general de la API, sin retornar información de conexión.
/// Relación: Cliente React -> GET /api/v1/plataforma/estado -> EstadoController.Obtener().
/// </summary>
[ApiController]
[Route("api/v1/plataforma/estado")]
public sealed class EstadoController : ControllerBase
{
    #region Consultas de plataforma
    /// <summary>
    /// Autor: Elmer Fuentes.
    /// Devuelve una respuesta básica para comprobar disponibilidad HTTP.
    /// </summary>
    [HttpGet]
    public IActionResult Obtener() => Ok(new { sistema = "EF Solutions", estado = "disponible" });
    #endregion
}
