using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UsuariosController(IConfiguration configuration, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Endpoint de Login dinámico contra la base de datos PostgreSQL.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var users = await _unitOfWork.Usuarios.FindAsync(u => u.Correo.ToLower() == request.Correo.ToLower());
        var usuario = users.FirstOrDefault();

        if (usuario == null)
        {
            return Unauthorized(new { Message = "Credenciales inválidas" });
        }

        // Validación simple en texto plano para los datos semilla y desarrollo
        if (usuario.PasswordHash != request.Password)
        {
            return Unauthorized(new { Message = "Credenciales inválidas" });
        }

        var token = GenerarTokenJWT(usuario.Id, usuario.Correo, usuario.Rol.ToString());
        
        return Ok(new { 
            Token = token,
            Rol = usuario.Rol.ToString(),
            Correo = usuario.Correo
        });
    }

    /// <summary>
    /// Lista todos los usuarios registrados (Exclusivo de Admin) con paginación.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var all = await _unitOfWork.Usuarios.GetAllAsync();
        var paged = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        
        Response.Headers.Append("X-Total-Count", all.Count().ToString());
        return Ok(_mapper.Map<IEnumerable<UsuarioDto>>(paged));
    }

    /// <summary>
    /// Obtiene un usuario específico por su ID (Exclusivo de Admin).
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
        if (usuario == null) return NotFound(new { Message = $"Usuario con Id {id} no encontrado." });
        return Ok(_mapper.Map<UsuarioDto>(usuario));
    }

    /// <summary>
    /// Crea un nuevo usuario en el sistema (Exclusivo de Admin).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateUsuarioDto dto)
    {
        var existing = await _unitOfWork.Usuarios.FindAsync(u => u.Correo.ToLower() == dto.Correo.ToLower());
        if (existing.Any()) return BadRequest(new { Message = $"El correo '{dto.Correo}' ya está registrado." });

        if (!Enum.TryParse<RolUsuario>(dto.Rol, true, out var parsedRol))
        {
            return BadRequest(new { Message = $"El rol '{dto.Rol}' no es válido. Roles válidos: Admin, Mecanico, Recepcionista" });
        }

        var usuario = new Usuario
        {
            Correo = dto.Correo,
            PasswordHash = dto.Password, // Guardar password directo
            Rol = parsedRol
        };

        await _unitOfWork.Usuarios.AddAsync(usuario);
        await _unitOfWork.CommitAsync();

        return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, _mapper.Map<UsuarioDto>(usuario));
    }

    /// <summary>
    /// Actualiza los datos o contraseña de un usuario existente (Exclusivo de Admin).
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateUsuarioDto dto)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
        if (usuario == null) return NotFound(new { Message = $"Usuario con Id {id} no encontrado." });

        // Validar correo único si cambia
        if (usuario.Correo.ToLower() != dto.Correo.ToLower())
        {
            var existing = await _unitOfWork.Usuarios.FindAsync(u => u.Correo.ToLower() == dto.Correo.ToLower());
            if (existing.Any()) return BadRequest(new { Message = $"El correo '{dto.Correo}' ya está registrado." });
        }

        if (!Enum.TryParse<RolUsuario>(dto.Rol, true, out var parsedRol))
        {
            return BadRequest(new { Message = $"El rol '{dto.Rol}' no es válido. Roles válidos: Admin, Mecanico, Recepcionista" });
        }

        usuario.Correo = dto.Correo;
        usuario.Rol = parsedRol;
        
        if (!string.IsNullOrEmpty(dto.Password))
        {
            usuario.PasswordHash = dto.Password;
        }

        _unitOfWork.Usuarios.Update(usuario);
        await _unitOfWork.CommitAsync();

        return Ok(_mapper.Map<UsuarioDto>(usuario));
    }

    /// <summary>
    /// Elimina un usuario del sistema (Exclusivo de Admin).
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
        if (usuario == null) return NotFound(new { Message = $"Usuario con Id {id} no encontrado." });

        _unitOfWork.Usuarios.Remove(usuario);
        await _unitOfWork.CommitAsync();

        return NoContent();
    }

    private string GenerarTokenJWT(int userId, string email, string role)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Role, role)
            }),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["DurationInMinutes"]!)),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}

public class LoginRequest
{
    public string Correo { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
