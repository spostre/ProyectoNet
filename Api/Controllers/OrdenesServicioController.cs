using Application.Interfaces;
using Api.Interfaces;
using Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Controlador que gestiona el ciclo de vida completo de las Órdenes de Servicio.
/// Las órdenes siguen esta secuencia de estados: Pendiente → Aprobada → EnReparacion → Cerrada.
/// 
/// Reglas de acceso por rol:
///  - Recepcionista/Admin: puede crear y aprobar órdenes.
///  - Mecánico/Admin: puede actualizar el trabajo realizado y generar facturas.
///  - Todos los autenticados: pueden consultar órdenes.
/// </summary>
public class OrdenesServicioController : ControllerBase
{
    private readonly IOrdenServicioService _ordenServicioService; // Servicio de negocio con la lógica de órdenes
    private readonly IUnitOfWork _unitOfWork;                     // Acceso genérico a datos

    public OrdenesServicioController(IOrdenServicioService ordenServicioService, IUnitOfWork unitOfWork)
    {
        _ordenServicioService = ordenServicioService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Crea una nueva orden de servicio en estado "Pendiente".
    /// El servicio valida que el vehículo exista y que no tenga una orden activa.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Recepcionista,Admin")]
    public async Task<IActionResult> CrearOrden([FromBody] CreateOrdenServicioDto dto)
    {
        try
        {
            var result = await _ordenServicioService.CrearOrdenServicioAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza los detalles (repuestos usados y mano de obra) de una orden existente,
    /// y cambia su estado al valor indicado en nuevoEstado.
    /// Solo el Mecánico (o Admin) puede registrar el trabajo realizado.
    /// </summary>
    /// <param name="id">ID de la orden de servicio a actualizar.</param>
    /// <param name="repuestos">Lista de repuestos y cantidades utilizadas en la reparación.</param>
    /// <param name="nuevoEstado">Nuevo estado de la orden (ej. "EnReparacion", "Cerrada").</param>
    [HttpPut("{id}/trabajo")]
    [Authorize(Roles = "Mecanico,Admin")]
    public async Task<IActionResult> ActualizarTrabajo(int id, [FromBody] List<DetalleOrdenDto> repuestos, [FromQuery] string nuevoEstado)
    {
        var result = await _ordenServicioService.ActualizarOrdenConTrabajoRealizadoAsync(id, nuevoEstado, repuestos);
        if (!result) return NotFound();
        return Ok(new { Message = "Trabajo actualizado correctamente." });
    }

    /// <summary>
    /// Cambia el estado de la orden de "Pendiente" a "Aprobada" (cliente aprobó el presupuesto).
    /// A partir de "Aprobada", el mecánico puede comenzar la reparación.
    /// </summary>
    [HttpPut("{id}/aprobar")]
    [Authorize(Roles = "Recepcionista,Admin")]
    public async Task<IActionResult> AprobarOrden(int id)
    {
        var result = await _ordenServicioService.AprobarOrdenAsync(id);
        if (!result) return NotFound(new { Message = $"Orden con Id {id} no encontrada." });
        return Ok(new { Message = "Orden aprobada por el cliente. Estado actualizado a EnReparacion." });
    }

    /// <summary>
    /// Genera la factura final de una orden de servicio cerrada.
    /// Calcula el total de mano de obra + repuestos y crea el registro de Factura.
    /// Solo Mecánico/Admin puede facturar (quien cierra el trabajo genera la factura).
    /// </summary>
    [HttpPost("{id}/facturar")]
    [Authorize(Roles = "Mecanico,Admin")]
    public async Task<IActionResult> GenerarFactura(int id)
    {
        try
        {
            var factura = await _ordenServicioService.GenerarFacturaAsync(id);
            return Ok(factura);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Lista todas las órdenes de servicio con paginación.
    /// El header de respuesta X-Total-Count indica el total sin paginar.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var all = await _unitOfWork.OrdenesServicio.GetAllAsync();
        // Aplicar paginación: saltar registros previos y tomar la página solicitada
        var paged = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        
        Response.Headers.Append("X-Total-Count", all.Count().ToString()); // Total para el frontend
        return Ok(paged);
    }
}
