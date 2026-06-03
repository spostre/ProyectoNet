using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiculosController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public VehiculosController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Lista todos los vehículos con paginación y filtros por Cliente o VIN.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? clienteId = null, 
        [FromQuery] string? vin = null,
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10)
    {
        var all = await _unitOfWork.Vehiculos.GetAllAsync();

        if (clienteId.HasValue)
        {
            all = all.Where(v => v.ClienteId == clienteId.Value);
        }

        if (!string.IsNullOrEmpty(vin))
        {
            all = all.Where(v => v.Vin.Contains(vin, StringComparison.OrdinalIgnoreCase));
        }

        var paged = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        Response.Headers.Append("X-Total-Count", all.Count().ToString());
        return Ok(_mapper.Map<IEnumerable<VehiculoDto>>(paged));
    }

    /// <summary>
    /// Obtiene un vehículo por su ID.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(id);
        if (vehiculo == null) return NotFound(new { Message = $"Vehículo con Id {id} no encontrado." });
        return Ok(_mapper.Map<VehiculoDto>(vehiculo));
    }

    /// <summary>
    /// Registra un nuevo vehículo para un cliente existente.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Recepcionista")]
    public async Task<IActionResult> Create([FromBody] CreateVehiculoDto dto, [FromQuery] int clienteId)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(clienteId);
        if (cliente == null) return BadRequest(new { Message = $"El cliente con Id {clienteId} no existe." });

        var vehiculo = _mapper.Map<Vehiculo>(dto);
        vehiculo.AsignarCliente(clienteId);

        await _unitOfWork.Vehiculos.AddAsync(vehiculo);
        await _unitOfWork.CommitAsync();

        return CreatedAtAction(nameof(GetById), new { id = vehiculo.Id }, _mapper.Map<VehiculoDto>(vehiculo));
    }

    /// <summary>
    /// Edita la información de un vehículo existente.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Recepcionista")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateVehiculoDto dto)
    {
        var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(id);
        if (vehiculo == null) return NotFound(new { Message = $"Vehículo con Id {id} no encontrado." });

        vehiculo.Marca = dto.Marca;
        vehiculo.Modelo = dto.Modelo;
        vehiculo.Anio = dto.Anio;
        vehiculo.Vin = dto.Vin;
        vehiculo.Kilometraje = dto.Kilometraje;

        _unitOfWork.Vehiculos.Update(vehiculo);
        await _unitOfWork.CommitAsync();

        return Ok(_mapper.Map<VehiculoDto>(vehiculo));
    }

    /// <summary>
    /// Elimina un vehículo si no posee órdenes de servicio activas.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(id);
        if (vehiculo == null) return NotFound(new { Message = $"Vehículo con Id {id} no encontrado." });

        // Prevenir borrado si existen órdenes de servicio activas
        var ordenes = await _unitOfWork.OrdenesServicio.FindAsync(o => o.VehiculoId == id);
        var activas = ordenes.Where(o => o.Estado != EstadoOrden.Cerrada && o.Estado != EstadoOrden.Cancelada);
        if (activas.Any())
        {
            return BadRequest(new { Message = "No se puede eliminar el vehículo porque posee órdenes de servicio activas en el taller." });
        }

        _unitOfWork.Vehiculos.Remove(vehiculo);
        await _unitOfWork.CommitAsync();

        return NoContent();
    }
}
