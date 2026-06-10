# 🧱 Moldes de Código — Examen ProyectoNet

> **Cómo usar esto:** Busca el molde que necesitas, copia, reemplaza `[NombreEntidad]` y los campos por los tuyos.

---

## 📋 Índice
1. [Entidad (Domain)](#1-entidad-domain)
2. [DTO de entrada (Create)](#2-dto-de-entrada-create)
3. [DTO de salida (Read)](#3-dto-de-salida-read)
4. [Mapping en AutoMapper](#4-mapping-en-automapper)
5. [DbSet en DbContext](#5-dbset-en-dbcontext)
6. [IUnitOfWork](#6-iunitofwork)
7. [UnitOfWork](#7-unitofwork)
8. [Controller CRUD completo](#8-controller-crud-completo)
9. [Controller solo GET + POST](#9-controller-solo-get--post)
10. [Endpoint con filtro (WHERE)](#10-endpoint-con-filtro-where)
11. [Endpoint con relación anidada](#11-endpoint-con-relación-anidada)
12. [Servicio de negocio](#12-servicio-de-negocio)
13. [Interfaz de servicio](#13-interfaz-de-servicio)
14. [Enum](#14-enum)
15. [Comandos de migración](#15-comandos-de-migración)

---

## 1. Entidad (Domain)

**Archivo:** `Domain/Entities/[NombreEntidad].cs`

```csharp
namespace Domain.Entities;

public class [NombreEntidad] : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }

    // Relación con otra entidad (FK)
    public int [OtraEntidad]Id { get; set; }
    public [OtraEntidad]? [OtraEntidad] { get; set; }

    // Si tiene hijos (1:N)
    public ICollection<[EntidadHija]> [EntidadesHijas] { get; set; } = new List<[EntidadHija]>();
}
```

**Versión mínima (solo nombre):**
```csharp
namespace Domain.Entities;

public class [NombreEntidad] : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
}
```

---

## 2. DTO de entrada (Create)

**Archivo:** `Api/DTOs/Create[NombreEntidad]Dto.cs`

```csharp
namespace Api.DTOs;

public class Create[NombreEntidad]Dto
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int [OtraEntidad]Id { get; set; } // FK
}
```

---

## 3. DTO de salida (Read)

**Archivo:** `Api/DTOs/[NombreEntidad]Dto.cs`

```csharp
namespace Api.DTOs;

public class [NombreEntidad]Dto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }

    // Campo desnormalizado (viene de otra tabla, se mapea en AutoMapper)
    public string [OtraEntidad]Nombre { get; set; } = string.Empty;
}
```

---

## 4. Mapping en AutoMapper

**Archivo:** `Api/Mappings/AutoMapperProfile.cs`  
**Agregar dentro del constructor de `AutoMapperProfile`:**

```csharp
// Mapeo simple bidireccional
CreateMap<[NombreEntidad], [NombreEntidad]Dto>().ReverseMap();

// Mapeo de entrada (Create)
CreateMap<Create[NombreEntidad]Dto, [NombreEntidad]>();

// Mapeo con campo de otra tabla (desnormalizado)
CreateMap<[NombreEntidad], [NombreEntidad]Dto>()
    .ForMember(dest => dest.[OtraEntidad]Nombre,
               opt => opt.MapFrom(src => src.[OtraEntidad] != null ? src.[OtraEntidad].Nombre : string.Empty))
    .ReverseMap();
```

---

## 5. DbSet en DbContext

**Archivo:** `Infrastructure/Data/AutoTallerDbContext.cs`  
**Agregar la propiedad DbSet:**

```csharp
public DbSet<[NombreEntidad]> [NombreEntidades] { get; set; }
```

**Agregar relación en `OnModelCreating` (si hay FK):**

```csharp
// Relación [NombreEntidad] → [OtraEntidad] (N:1)
modelBuilder.Entity<[NombreEntidad]>()
    .HasOne(x => x.[OtraEntidad])
    .WithMany(o => o.[NombreEntidades])
    .HasForeignKey(x => x.[OtraEntidad]Id)
    .OnDelete(DeleteBehavior.Restrict);
```

---

## 6. IUnitOfWork

**Archivo:** `Application/Interfaces/IUnitOfWork.cs`  
**Agregar la propiedad:**

```csharp
IGenericRepository<[NombreEntidad]> [NombreEntidades] { get; }
```

---

## 7. UnitOfWork

**Archivo:** `Infrastructure/UnitOfWork/UnitOfWork.cs`  
**Agregar la propiedad:**

```csharp
public IGenericRepository<[NombreEntidad]> [NombreEntidades] { get; private set; }
```

**Agregar en el constructor:**

```csharp
[NombreEntidades] = new GenericRepository<[NombreEntidad]>(_context);
```

---

## 8. Controller CRUD completo

**Archivo:** `Api/Controllers/[NombreEntidad]sController.cs`

```csharp
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
public class [NombreEntidad]sController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public [NombreEntidad]sController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>Lista todos los registros con paginación.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var all = await _unitOfWork.[NombreEntidades].GetAllAsync();
        var paged = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        Response.Headers.Append("X-Total-Count", all.Count().ToString());
        return Ok(_mapper.Map<IEnumerable<[NombreEntidad]Dto>>(paged));
    }

    /// <summary>Obtiene un registro por ID.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var entidad = await _unitOfWork.[NombreEntidades].GetByIdAsync(id);
        if (entidad == null) return NotFound(new { Message = $"[NombreEntidad] con Id {id} no encontrado." });
        return Ok(_mapper.Map<[NombreEntidad]Dto>(entidad));
    }

    /// <summary>Crea un nuevo registro. Solo Admin.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Create[NombreEntidad]Dto dto)
    {
        try
        {
            var entidad = _mapper.Map<[NombreEntidad]>(dto);
            await _unitOfWork.[NombreEntidades].AddAsync(entidad);
            await _unitOfWork.CommitAsync();
            return CreatedAtAction(nameof(GetById), new { id = entidad.Id }, _mapper.Map<[NombreEntidad]Dto>(entidad));
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>Actualiza un registro existente. Solo Admin.</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] Create[NombreEntidad]Dto dto)
    {
        var entidad = await _unitOfWork.[NombreEntidades].GetByIdAsync(id);
        if (entidad == null) return NotFound(new { Message = $"[NombreEntidad] con Id {id} no encontrado." });

        entidad.Nombre = dto.Nombre;
        entidad.Descripcion = dto.Descripcion;
        // ... otros campos

        _unitOfWork.[NombreEntidades].Update(entidad);
        await _unitOfWork.CommitAsync();
        return Ok(_mapper.Map<[NombreEntidad]Dto>(entidad));
    }

    /// <summary>Elimina un registro. Solo Admin.</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var entidad = await _unitOfWork.[NombreEntidades].GetByIdAsync(id);
        if (entidad == null) return NotFound(new { Message = $"[NombreEntidad] con Id {id} no encontrado." });

        _unitOfWork.[NombreEntidades].Remove(entidad);
        await _unitOfWork.CommitAsync();
        return NoContent();
    }
}
```

---

## 9. Controller solo GET + POST

Versión reducida cuando el examen solo pide consultar e insertar:

```csharp
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
public class [NombreEntidad]sController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public [NombreEntidad]sController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var lista = await _unitOfWork.[NombreEntidades].GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<[NombreEntidad]Dto>>(lista));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Create[NombreEntidad]Dto dto)
    {
        var entidad = _mapper.Map<[NombreEntidad]>(dto);
        await _unitOfWork.[NombreEntidades].AddAsync(entidad);
        await _unitOfWork.CommitAsync();
        return Ok(_mapper.Map<[NombreEntidad]Dto>(entidad));
    }
}
```

---

## 10. Endpoint con filtro (WHERE)

Útil cuando necesitas traer registros filtrados por un campo:

```csharp
/// <summary>Filtra registros por un campo específico.</summary>
[HttpGet("buscar")]
public async Task<IActionResult> BuscarPor([FromQuery] string nombre)
{
    // FindAsync equivale a: SELECT * FROM tabla WHERE Nombre LIKE '%nombre%'
    var resultados = await _unitOfWork.[NombreEntidades]
        .FindAsync(x => x.Nombre.Contains(nombre));
    return Ok(_mapper.Map<IEnumerable<[NombreEntidad]Dto>>(resultados));
}

// Filtrar por FK (traer hijos de un padre)
[HttpGet("{id}/[hijos]")]
public async Task<IActionResult> GetHijos(int id)
{
    // WHERE PadreId = id
    var hijos = await _unitOfWork.[EntidadesHijas]
        .FindAsync(x => x.[NombreEntidad]Id == id);
    return Ok(_mapper.Map<IEnumerable<[EntidadHija]Dto>>(hijos));
}
```

---

## 11. Endpoint con relación anidada

Cuando necesitas insertar un padre con sus hijos en una sola llamada:

```csharp
[HttpPost("con-detalles")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> CrearConDetalles([FromBody] Create[NombreEntidad]Dto dto)
{
    // 1. Crear y guardar el padre
    var padre = new [NombreEntidad]
    {
        Nombre = dto.Nombre,
        Descripcion = dto.Descripcion
    };
    await _unitOfWork.[NombreEntidades].AddAsync(padre);
    await _unitOfWork.CommitAsync(); // Necesario para obtener el ID del padre

    // 2. Crear los hijos vinculados al padre
    foreach (var detalleDto in dto.Detalles)
    {
        var detalle = new [EntidadHija]
        {
            Nombre = detalleDto.Nombre,
            [NombreEntidad]Id = padre.Id // Vincular al padre recién creado
        };
        await _unitOfWork.[EntidadesHijas].AddAsync(detalle);
    }

    await _unitOfWork.CommitAsync(); // Guardar todos los hijos
    return Ok(new { Message = "Creado correctamente.", Id = padre.Id });
}
```

---

## 12. Servicio de negocio

Cuando la lógica es demasiado compleja para el controller:

**Archivo:** `Api/Services/[NombreEntidad]Service.cs`

```csharp
using Api.DTOs;
using Api.Interfaces;
using Application.Interfaces;
using Domain.Entities;

namespace Api.Services;

public class [NombreEntidad]Service : I[NombreEntidad]Service
{
    private readonly IUnitOfWork _unitOfWork;

    public [NombreEntidad]Service(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<[NombreEntidad]Dto> Crear[NombreEntidad]Async(Create[NombreEntidad]Dto dto)
    {
        // Validación de negocio
        var existente = await _unitOfWork.[NombreEntidades]
            .FindAsync(x => x.Nombre == dto.Nombre);
        if (existente.Any())
            throw new InvalidOperationException($"Ya existe un registro con el nombre '{dto.Nombre}'.");

        // Crear entidad
        var entidad = new [NombreEntidad]
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion
        };

        await _unitOfWork.[NombreEntidades].AddAsync(entidad);
        await _unitOfWork.CommitAsync();

        return new [NombreEntidad]Dto
        {
            Id = entidad.Id,
            Nombre = entidad.Nombre,
            Descripcion = entidad.Descripcion
        };
    }
}
```

---

## 13. Interfaz de servicio

**Archivo:** `Api/Interfaces/I[NombreEntidad]Service.cs`

```csharp
using Api.DTOs;

namespace Api.Interfaces;

public interface I[NombreEntidad]Service
{
    Task<[NombreEntidad]Dto> Crear[NombreEntidad]Async(Create[NombreEntidad]Dto dto);
    // Agrega más métodos según necesites:
    // Task<bool> Eliminar[NombreEntidad]Async(int id);
}
```

**Registrar en `Program.cs`** (agregar junto a los otros AddScoped):
```csharp
builder.Services.AddScoped<I[NombreEntidad]Service, [NombreEntidad]Service>();
```

---

## 14. Enum

**Archivo:** `Domain/Enums/[NombreEnum].cs`

```csharp
namespace Domain.Enums;

public enum [NombreEnum]
{
    Opcion1,
    Opcion2,
    Opcion3
}
```

**Usar en una entidad:**
```csharp
using Domain.Enums;

public class MiEntidad : BaseEntity
{
    public [NombreEnum] Estado { get; set; } = [NombreEnum].Opcion1;
}
```

**Convertir a string en mapping** (AutoMapperProfile):
```csharp
CreateMap<MiEntidad, MiEntidadDto>()
    .ForMember(dest => dest.Estado,
               opt => opt.MapFrom(src => src.Estado.ToString()))
    .ReverseMap();
```

---

## 15. Comandos de migración

> ⚠️ **Ejecutar desde la carpeta raíz del proyecto** (donde están las carpetas Api, Domain, etc.)

```powershell
# Crear una nueva migración (SIEMPRE después de cambiar entidades o DbContext)
dotnet ef migrations add [NombreMigracion] -p Infrastructure -s Api

# Aplicar la migración a la base de datos manualmente
dotnet ef database update -p Infrastructure -s Api

# Ver lista de migraciones
dotnet ef migrations list -p Infrastructure -s Api

# Eliminar la última migración (si te equivocaste)
dotnet ef migrations remove -p Infrastructure -s Api
```

> **Nota:** En este proyecto la migración se aplica automáticamente al iniciar (`Program.cs → context.Database.Migrate()`), así que el `database update` manual normalmente no es necesario. Solo necesitas crear la migración.

---

## 🚀 Checklist rápido para agregar una entidad nueva

```
[ ] 1. Crear Domain/Entities/[Entidad].cs
[ ] 2. Agregar DbSet en AutoTallerDbContext.cs
[ ] 3. Agregar relación en OnModelCreating (si tiene FK)
[ ] 4. Agregar propiedad en IUnitOfWork.cs
[ ] 5. Instanciar en constructor de UnitOfWork.cs
[ ] 6. Crear Api/DTOs/[Entidad]Dto.cs
[ ] 7. Crear Api/DTOs/Create[Entidad]Dto.cs
[ ] 8. Agregar CreateMap en AutoMapperProfile.cs
[ ] 9. Crear Api/Controllers/[Entidad]sController.cs
[ ] 10. dotnet ef migrations add Agregar[Entidad] -p Infrastructure -s Api
[ ] 11. dotnet run (la migración se aplica sola)
```

---

## 🔑 Resumen de atributos de controller

| Atributo | Significado |
|---|---|
| `[HttpGet]` | GET /api/entidades |
| `[HttpGet("{id}")]` | GET /api/entidades/5 |
| `[HttpPost]` | POST /api/entidades |
| `[HttpPut("{id}")]` | PUT /api/entidades/5 |
| `[HttpDelete("{id}")]` | DELETE /api/entidades/5 |
| `[Authorize]` | Requiere cualquier JWT válido |
| `[Authorize(Roles = "Admin")]` | Solo Admin |
| `[Authorize(Roles = "Admin,Mecanico")]` | Admin o Mecánico |
| `[FromBody]` | Dato viene del JSON del body |
| `[FromQuery]` | Dato viene de la URL (?param=valor) |

## 🔑 Resumen de respuestas HTTP

| Código | Método | Cuándo usarlo |
|---|---|---|
| `return Ok(data)` | 200 | Éxito con datos |
| `return CreatedAtAction(...)` | 201 | Recurso creado exitosamente |
| `return NoContent()` | 204 | Éxito sin datos (ej. DELETE) |
| `return NotFound(...)` | 404 | Recurso no encontrado |
| `return BadRequest(...)` | 400 | Error de validación / regla de negocio |
| `return Unauthorized()` | 401 | Sin token o token inválido |
