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
/// Controlador para gestionar el Catálogo de Servicios del taller (ej. "Cambio de aceite", "Revisión general").
/// Los servicios del catálogo se usan al crear DetallesOrden para registrar la mano de obra aplicada.
/// Solo Admin puede crear/modificar/eliminar servicios del catálogo.
/// </summary>
public class CatalogoServiciosController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CatalogoServiciosController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>Lista todos los tipos de servicio disponibles en el catálogo.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var servicios = await _unitOfWork.CatalogoServicios.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<CatalogoServicioDto>>(servicios));
    }

    /// <summary>Obtiene un tipo de servicio por ID.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var servicio = await _unitOfWork.CatalogoServicios.GetByIdAsync(id);
        if (servicio == null) return NotFound(new { Message = $"Servicio con Id {id} no encontrado." });
        return Ok(_mapper.Map<CatalogoServicioDto>(servicio));
    }

    /// <summary>
    /// Crea un nuevo tipo de servicio en el catálogo. Solo Admin.
    /// PrecioManoObra es el costo base del servicio independiente de los repuestos.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CatalogoServicioDto dto)
    {
        var servicio = new CatalogoServicio
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            PrecioManoObra = dto.PrecioManoObra // Costo del trabajo (sin repuestos)
        };
        await _unitOfWork.CatalogoServicios.AddAsync(servicio);
        await _unitOfWork.CommitAsync();
        return CreatedAtAction(nameof(GetById), new { id = servicio.Id }, _mapper.Map<CatalogoServicioDto>(servicio));
    }

    /// <summary>Actualiza nombre, descripción y precio de un servicio del catálogo. Solo Admin.</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CatalogoServicioDto dto)
    {
        var servicio = await _unitOfWork.CatalogoServicios.GetByIdAsync(id);
        if (servicio == null) return NotFound(new { Message = $"Servicio con Id {id} no encontrado." });
        servicio.Nombre = dto.Nombre;
        servicio.Descripcion = dto.Descripcion;
        servicio.PrecioManoObra = dto.PrecioManoObra;
        _unitOfWork.CatalogoServicios.Update(servicio);
        await _unitOfWork.CommitAsync();
        return Ok(_mapper.Map<CatalogoServicioDto>(servicio));
    }

    /// <summary>Elimina un servicio del catálogo. Solo Admin.</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var servicio = await _unitOfWork.CatalogoServicios.GetByIdAsync(id);
        if (servicio == null) return NotFound(new { Message = $"Servicio con Id {id} no encontrado." });
        _unitOfWork.CatalogoServicios.Remove(servicio);
        await _unitOfWork.CommitAsync();
        return NoContent();
    }
}
