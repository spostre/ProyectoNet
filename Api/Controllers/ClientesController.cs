using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;
    private readonly IUnitOfWork _unitOfWork;

    public ClientesController(IClienteService clienteService, IUnitOfWork unitOfWork)
    {
        _clienteService = clienteService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Registra un nuevo cliente junto con uno o más vehículos asociados en una sola operación.
    /// </summary>
    [HttpPost("registrar-con-vehiculo")]
    [Authorize(Roles = "Admin,Recepcionista")]
    public async Task<IActionResult> RegistrarConVehiculo([FromBody] CreateClienteDto dto)
    {
        try
        {
            var result = await _clienteService.RegistrarClienteConVehiculoAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Añade uno o más vehículos a un cliente ya existente.
    /// </summary>
    [HttpPost("{id}/vehiculos")]
    [Authorize(Roles = "Admin,Recepcionista")]
    public async Task<IActionResult> AgregarVehiculos(int id, [FromBody] ICollection<CreateVehiculoDto> vehiculosDto)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
        if (cliente == null) return NotFound(new { Message = $"Cliente con Id {id} no encontrado." });

        var vehiculosCreados = new List<Domain.Entities.Vehiculo>();
        foreach (var dto in vehiculosDto)
        {
            var vehiculo = new Domain.Entities.Vehiculo
            {
                Marca       = dto.Marca,
                Modelo      = dto.Modelo,
                Anio        = dto.Anio,
                Vin         = dto.Vin,
                Kilometraje = dto.Kilometraje
            };
            vehiculo.AsignarCliente(id);
            await _unitOfWork.Vehiculos.AddAsync(vehiculo);
            vehiculosCreados.Add(vehiculo);
        }

        await _unitOfWork.CommitAsync();
        return Ok(new { Message = $"{vehiculosCreados.Count} vehículo(s) agregado(s) al cliente Id {id}.", Vehiculos = vehiculosCreados.Select(v => new { v.Id, v.Marca, v.Modelo, v.Vin }) });
    }

    /// <summary>
    /// Obtiene todos los clientes registrados con paginación.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var all = await _unitOfWork.Clientes.GetAllAsync();
        var paged = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        Response.Headers.Append("X-Total-Count", all.Count().ToString());
        return Ok(paged);
    }

    /// <summary>
    /// Obtiene un cliente por su ID.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
        if (cliente == null) return NotFound(new { Message = $"Cliente con Id {id} no encontrado." });
        return Ok(cliente);
    }

    /// <summary>
    /// Actualiza la información de contacto de un cliente.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Recepcionista")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateClienteDto dto)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
        if (cliente == null) return NotFound(new { Message = $"Cliente con Id {id} no encontrado." });

        cliente.Nombre = dto.Nombre;
        cliente.Telefono = dto.Telefono;
        cliente.Correo = dto.Correo;

        _unitOfWork.Clientes.Update(cliente);
        await _unitOfWork.CommitAsync();

        return Ok(cliente);
    }

    /// <summary>
    /// Elimina un cliente si ninguno de sus vehículos posee órdenes de servicio activas.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
        if (cliente == null) return NotFound(new { Message = $"Cliente con Id {id} no encontrado." });

        // Obtener los vehículos del cliente
        var vehiculos = await _unitOfWork.Vehiculos.FindAsync(v => v.ClienteId == id);
        foreach (var vehiculo in vehiculos)
        {
            var ordenes = await _unitOfWork.OrdenesServicio.FindAsync(o => o.VehiculoId == vehiculo.Id);
            var activas = ordenes.Where(o => o.Estado != Domain.Enums.EstadoOrden.Cerrada && o.Estado != Domain.Enums.EstadoOrden.Cancelada);
            if (activas.Any())
            {
                return BadRequest(new { Message = $"No se puede eliminar el cliente porque su vehículo '{vehiculo.Marca} {vehiculo.Modelo}' (VIN: {vehiculo.Vin}) posee órdenes de servicio activas." });
            }
        }

        // Si no hay órdenes activas, primero eliminamos o desvinculamos los vehículos (EF Core se encargará según la relación)
        // Como borrado es Restrict en relación, eliminamos manualmente los vehículos del cliente
        foreach (var vehiculo in vehiculos)
        {
            _unitOfWork.Vehiculos.Remove(vehiculo);
        }

        _unitOfWork.Clientes.Remove(cliente);
        await _unitOfWork.CommitAsync();

        return NoContent();
    }
}
