using Api.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Todos los endpoints requieren autenticación; algunos exigen rol Admin
/// <summary>
/// Controlador para gestionar Marcas de vehículos (ej. Toyota, Honda).
/// Las marcas agrupan Modelos. Requieren autenticación en todas las operaciones.
/// Solo Admin puede crear, modificar o eliminar marcas.
/// </summary>
public class MarcasController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MarcasController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>Retorna todas las marcas disponibles.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var marcas = await _unitOfWork.Marcas.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<MarcaDto>>(marcas));
    }

    /// <summary>Retorna una marca específica por su ID.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var marca = await _unitOfWork.Marcas.GetByIdAsync(id);
        if (marca == null) return NotFound(new { Message = $"Marca con Id {id} no encontrada." });
        return Ok(_mapper.Map<MarcaDto>(marca));
    }

    /// <summary>
    /// Retorna todos los modelos que pertenecen a una marca.
    /// Útil para los formularios de registro de vehículos: primero seleccionar marca, luego modelo.
    /// </summary>
    [HttpGet("{id}/modelos")]
    public async Task<IActionResult> GetModelos(int id)
    {
        var modelos = await _unitOfWork.Modelos.FindAsync(m => m.MarcaId == id); // WHERE MarcaId = id
        return Ok(_mapper.Map<IEnumerable<ModeloDto>>(modelos));
    }

    /// <summary>Crea una nueva marca. Solo Admin.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] MarcaDto dto)
    {
        var marca = new Marca { Nombre = dto.Nombre };
        await _unitOfWork.Marcas.AddAsync(marca);
        await _unitOfWork.CommitAsync();
        return CreatedAtAction(nameof(GetById), new { id = marca.Id }, _mapper.Map<MarcaDto>(marca));
    }

    /// <summary>Actualiza el nombre de una marca existente. Solo Admin.</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] MarcaDto dto)
    {
        var marca = await _unitOfWork.Marcas.GetByIdAsync(id);
        if (marca == null) return NotFound(new { Message = $"Marca con Id {id} no encontrada." });
        marca.Nombre = dto.Nombre;
        _unitOfWork.Marcas.Update(marca);
        await _unitOfWork.CommitAsync();
        return Ok(_mapper.Map<MarcaDto>(marca));
    }

    /// <summary>Elimina una marca. Solo Admin. Advertencia: no se puede borrar si tiene modelos asociados (FK).</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var marca = await _unitOfWork.Marcas.GetByIdAsync(id);
        if (marca == null) return NotFound(new { Message = $"Marca con Id {id} no encontrada." });
        _unitOfWork.Marcas.Remove(marca);
        await _unitOfWork.CommitAsync();
        return NoContent();
    }
}
