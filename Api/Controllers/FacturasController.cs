using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacturasController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public FacturasController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Consulta el histórico de facturas de forma paginada con filtros opcionales por cliente, orden de servicio o rango de fechas.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? clienteId = null,
        [FromQuery] int? ordenServicioId = null,
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var all = await _unitOfWork.Facturas.GetAllAsync();

        // Aplicar filtros
        if (ordenServicioId.HasValue)
        {
            all = all.Where(f => f.OrdenServicioId == ordenServicioId.Value);
        }

        if (fechaInicio.HasValue)
        {
            all = all.Where(f => f.FechaGeneracion >= fechaInicio.Value);
        }

        if (fechaFin.HasValue)
        {
            all = all.Where(f => f.FechaGeneracion <= fechaFin.Value);
        }

        if (clienteId.HasValue)
        {
            // Para filtrar por cliente, cargamos los detalles de la orden
            var ordenes = await _unitOfWork.OrdenesServicio.GetAllAsync();
            var vehiculos = await _unitOfWork.Vehiculos.FindAsync(v => v.ClienteId == clienteId.Value);
            var vehiculosIds = vehiculos.Select(v => v.Id).ToHashSet();
            var ordenesIdsOfCliente = ordenes.Where(o => vehiculosIds.Contains(o.VehiculoId)).Select(o => o.Id).ToHashSet();

            all = all.Where(f => ordenesIdsOfCliente.Contains(f.OrdenServicioId));
        }

        var ordered = all.OrderByDescending(f => f.FechaGeneracion).ToList();
        var paged = ordered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        Response.Headers.Append("X-Total-Count", all.Count().ToString());
        return Ok(_mapper.Map<IEnumerable<FacturaDto>>(paged));
    }

    /// <summary>
    /// Obtiene una factura específica por su ID.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var factura = await _unitOfWork.Facturas.GetByIdAsync(id);
        if (factura == null) return NotFound(new { Message = $"Factura con Id {id} no encontrada." });
        return Ok(_mapper.Map<FacturaDto>(factura));
    }

    /// <summary>
    /// Obtiene la factura correspondiente a una orden de servicio.
    /// </summary>
    [HttpGet("orden/{ordenId}")]
    [Authorize]
    public async Task<IActionResult> GetByOrdenId(int ordenId)
    {
        var facturas = await _unitOfWork.Facturas.FindAsync(f => f.OrdenServicioId == ordenId);
        var factura = facturas.FirstOrDefault();
        if (factura == null) return NotFound(new { Message = $"No existe factura para la orden de servicio con Id {ordenId}." });
        return Ok(_mapper.Map<FacturaDto>(factura));
    }
}
