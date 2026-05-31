using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditoriasController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AuditoriasController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Obtiene el registro completo de auditorías (exclusivo de Admin) con paginación.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var all = await _unitOfWork.Auditorias.GetAllAsync();
        var ordered = all.OrderByDescending(a => a.FechaAccion).ToList();
        var paged = ordered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        
        Response.Headers.Append("X-Total-Count", all.Count().ToString());
        return Ok(_mapper.Map<IEnumerable<AuditoriaDto>>(paged));
    }
}
