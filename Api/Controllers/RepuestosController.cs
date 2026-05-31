using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RepuestosController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RepuestosController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Lista todos los repuestos del inventario con paginación.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var all = await _unitOfWork.Repuestos.GetAllAsync();
        var paged = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        var result = _mapper.Map<IEnumerable<RepuestoDto>>(paged);
        Response.Headers.Append("X-Total-Count", all.Count().ToString());
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un repuesto específico por su ID.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var repuesto = await _unitOfWork.Repuestos.GetByIdAsync(id);
        if (repuesto == null) return NotFound(new { Message = $"Repuesto con Id {id} no encontrado." });
        return Ok(_mapper.Map<RepuestoDto>(repuesto));
    }

    /// <summary>
    /// Registra un nuevo repuesto en el inventario.
    /// Solo Admin puede registrar repuestos.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateRepuestoDto dto)
    {
        try
        {
            var repuesto = _mapper.Map<Repuesto>(dto);
            await _unitOfWork.Repuestos.AddAsync(repuesto);
            await _unitOfWork.CommitAsync();
            return CreatedAtAction(nameof(GetById), new { id = repuesto.Id }, _mapper.Map<RepuestoDto>(repuesto));
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza los datos de un repuesto existente.
    /// Solo Admin puede modificar repuestos.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateRepuestoDto dto)
    {
        var repuesto = await _unitOfWork.Repuestos.GetByIdAsync(id);
        if (repuesto == null) return NotFound(new { Message = $"Repuesto con Id {id} no encontrado." });

        // Actualizar campos de forma explícita para respetar la lógica de dominio
        repuesto.Descripcion = dto.Descripcion;
        repuesto.PrecioUnitario = dto.PrecioUnitario;
        repuesto.Codigo = dto.Codigo;

        // Ajustar stock: si el nuevo valor es mayor, aumentar; si es menor, reducir
        if (dto.CantidadStock > repuesto.CantidadStock)
            repuesto.AumentarStock(dto.CantidadStock - repuesto.CantidadStock);
        else if (dto.CantidadStock < repuesto.CantidadStock)
            repuesto.ReducirStock(repuesto.CantidadStock - dto.CantidadStock);

        _unitOfWork.Repuestos.Update(repuesto);
        await _unitOfWork.CommitAsync();
        return Ok(_mapper.Map<RepuestoDto>(repuesto));
    }

    /// <summary>
    /// Elimina un repuesto del inventario.
    /// Solo Admin puede eliminar repuestos.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var repuesto = await _unitOfWork.Repuestos.GetByIdAsync(id);
        if (repuesto == null) return NotFound(new { Message = $"Repuesto con Id {id} no encontrado." });

        _unitOfWork.Repuestos.Remove(repuesto);
        await _unitOfWork.CommitAsync();
        return NoContent();
    }
}
