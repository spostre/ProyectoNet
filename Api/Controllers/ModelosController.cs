using Api.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
/// <summary>
/// Controlador para gestionar Modelos de vehículos (ej. Corolla, Civic).
/// Un Modelo siempre pertenece a una Marca (relación N:1).
/// Solo Admin puede crear/modificar/eliminar modelos.
/// </summary>
public class ModelosController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ModelosController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>Lista todos los modelos disponibles.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var modelos = await _unitOfWork.Modelos.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<ModeloDto>>(modelos));
    }

    /// <summary>Obtiene un modelo específico por ID.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var modelo = await _unitOfWork.Modelos.GetByIdAsync(id);
        if (modelo == null) return NotFound(new { Message = $"Modelo con Id {id} no encontrado." });
        return Ok(_mapper.Map<ModeloDto>(modelo));
    }

    /// <summary>
    /// Crea un nuevo modelo. Solo Admin.
    /// Se debe indicar el MarcaId al que pertenece (la marca debe existir previamente).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] ModeloDto dto)
    {
        var modelo = new Modelo { Nombre = dto.Nombre, MarcaId = dto.MarcaId }; // Asignar la marca al modelo
        await _unitOfWork.Modelos.AddAsync(modelo);
        await _unitOfWork.CommitAsync();
        return CreatedAtAction(nameof(GetById), new { id = modelo.Id }, _mapper.Map<ModeloDto>(modelo));
    }

    /// <summary>Actualiza nombre y marca de un modelo. Solo Admin.</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] ModeloDto dto)
    {
        var modelo = await _unitOfWork.Modelos.GetByIdAsync(id);
        if (modelo == null) return NotFound(new { Message = $"Modelo con Id {id} no encontrado." });
        modelo.Nombre = dto.Nombre;
        modelo.MarcaId = dto.MarcaId;
        _unitOfWork.Modelos.Update(modelo);
        await _unitOfWork.CommitAsync();
        return Ok(_mapper.Map<ModeloDto>(modelo));
    }

    /// <summary>Elimina un modelo. Solo Admin. Falla si tiene vehículos asociados.</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var modelo = await _unitOfWork.Modelos.GetByIdAsync(id);
        if (modelo == null) return NotFound(new { Message = $"Modelo con Id {id} no encontrado." });
        _unitOfWork.Modelos.Remove(modelo);
        await _unitOfWork.CommitAsync();
        return NoContent();
    }
}
