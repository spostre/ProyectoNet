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
/// Controlador para gestionar Proveedores de repuestos.
/// Los proveedores abastecen el inventario de repuestos del taller.
/// Solo Admin puede crear/modificar/eliminar proveedores.
/// Cualquier usuario autenticado puede consultar proveedores y sus repuestos.
/// </summary>
public class ProveedoresController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProveedoresController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>Lista todos los proveedores registrados.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var proveedores = await _unitOfWork.Proveedores.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<ProveedorDto>>(proveedores));
    }

    /// <summary>Obtiene un proveedor por ID.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(id);
        if (proveedor == null) return NotFound(new { Message = $"Proveedor con Id {id} no encontrado." });
        return Ok(_mapper.Map<ProveedorDto>(proveedor));
    }

    /// <summary>
    /// Retorna los repuestos suministrados por este proveedor.
    /// Filtrado mediante FindAsync con expresión lambda: WHERE ProveedorId = id.
    /// </summary>
    [HttpGet("{id}/repuestos")]
    public async Task<IActionResult> GetRepuestos(int id)
    {
        var repuestos = await _unitOfWork.Repuestos.FindAsync(r => r.ProveedorId == id);
        // Proyectar solo los campos necesarios (anónimo, sin exponer toda la entidad)
        return Ok(repuestos.Select(r => new { r.Id, r.Codigo, r.Descripcion, r.PrecioUnitario, r.CantidadStock }));
    }

    /// <summary>Crea un nuevo proveedor. Solo Admin.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] ProveedorDto dto)
    {
        var proveedor = new Proveedor
        {
            NombreEmpresa = dto.NombreEmpresa,
            Contacto = dto.Contacto,
            Telefono = dto.Telefono,
            Correo = dto.Correo
        };
        await _unitOfWork.Proveedores.AddAsync(proveedor);
        await _unitOfWork.CommitAsync();
        return CreatedAtAction(nameof(GetById), new { id = proveedor.Id }, _mapper.Map<ProveedorDto>(proveedor));
    }

    /// <summary>Actualiza los datos de un proveedor existente. Solo Admin.</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] ProveedorDto dto)
    {
        var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(id);
        if (proveedor == null) return NotFound(new { Message = $"Proveedor con Id {id} no encontrado." });
        proveedor.NombreEmpresa = dto.NombreEmpresa;
        proveedor.Contacto = dto.Contacto;
        proveedor.Telefono = dto.Telefono;
        proveedor.Correo = dto.Correo;
        _unitOfWork.Proveedores.Update(proveedor);
        await _unitOfWork.CommitAsync();
        return Ok(_mapper.Map<ProveedorDto>(proveedor));
    }

    /// <summary>Elimina un proveedor. Solo Admin. Puede fallar si tiene repuestos asociados (FK Restrict).</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(id);
        if (proveedor == null) return NotFound(new { Message = $"Proveedor con Id {id} no encontrado." });
        _unitOfWork.Proveedores.Remove(proveedor);
        await _unitOfWork.CommitAsync();
        return NoContent();
    }
}
