using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdenesServicioController : ControllerBase
{
    private readonly IOrdenServicioService _ordenServicioService;
    private readonly IUnitOfWork _unitOfWork;

    public OrdenesServicioController(IOrdenServicioService ordenServicioService, IUnitOfWork unitOfWork)
    {
        _ordenServicioService = ordenServicioService;
        _unitOfWork = unitOfWork;
    }

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

    [HttpPut("{id}/trabajo")]
    [Authorize(Roles = "Mecanico,Admin")]
    public async Task<IActionResult> ActualizarTrabajo(int id, [FromBody] List<DetalleOrdenDto> repuestos, [FromQuery] string nuevoEstado)
    {
        var result = await _ordenServicioService.ActualizarOrdenConTrabajoRealizadoAsync(id, nuevoEstado, repuestos);
        if (!result) return NotFound();
        return Ok(new { Message = "Trabajo actualizado correctamente." });
    }

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

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var all = await _unitOfWork.OrdenesServicio.GetAllAsync();
        var paged = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        
        Response.Headers.Append("X-Total-Count", all.Count().ToString());
        return Ok(paged);
    }
}
