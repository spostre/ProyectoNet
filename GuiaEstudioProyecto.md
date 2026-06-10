# Guía de Estudio y Manual de Arquitectura del Código - AutoTallerManager

Este documento sirve como un manual técnico detallado del proyecto **AutoTallerManager** diseñado para preparar exámenes, resolución de problemas y extensiones del sistema. Explica la estructura de archivos, el flujo de datos, la configuración de la base de datos y proporciona guías paso a paso para añadir nuevas funcionalidades.

---

## 📌 1. ÍNDICE DE EXPLORACIÓN RÁPIDA

El proyecto está organizado bajo los principios de **Clean Architecture** (Arquitectura Limpia) con un flujo de dependencias unidireccional para desacoplar la lógica de dominio de los detalles de infraestructura:

```
[ Domain ] ◄─── [ Application (Interfaces) ] ◄─── [ Infrastructure (Data/Repos) ] ◄─── [ Api (Controllers/DTOs/Services) ]
```

### Tabla de Rutas Clave de Archivos
| Capa | Ruta del Archivo | Propósito del Archivo |
|---|---|---|
| **Domain** | [`Domain/Entities/Cliente.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/Cliente.cs) | Registro de clientes. Relación 1:N con vehículos. |
| **Domain** | [`Domain/Entities/Vehiculo.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/Vehiculo.cs) | Autos del taller. Relación N:1 con Cliente y Modelo. |
| **Domain** | [`Domain/Entities/Marca.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/Marca.cs) | Catálogo de marcas de autos (ej: Toyota, Honda). |
| **Domain** | [`Domain/Entities/Modelo.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/Modelo.cs) | Catálogo de modelos de marcas (ej: Corolla ➔ Toyota). |
| **Domain** | [`Domain/Entities/CatalogoServicio.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/CatalogoServicio.cs) | Servicios ofrecidos y precio fijo de mano de obra. |
| **Domain** | [`Domain/Entities/Proveedor.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/Proveedor.cs) | Distribuidores de repuestos del taller. |
| **Domain** | [`Domain/Entities/Repuesto.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/Repuesto.cs) | Inventario físico de piezas. Controla el stock. |
| **Domain** | [`Domain/Entities/OrdenServicio.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/OrdenServicio.cs) | Hoja de taller. Lógica de negocio (aprobar, estimar fecha). |
| **Domain** | [`Domain/Entities/DetalleOrden.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/DetalleOrden.cs) | Repuestos asociados a una orden. Calcula subtotales. |
| **Domain** | [`Domain/Entities/Factura.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/Factura.cs) | Comprobante de cierre de orden. Suma repuestos y mano de obra. |
| **Domain** | [`Domain/Entities/Usuario.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/Usuario.cs) | Credenciales del personal del taller (Admin, Mecánico, Recep). |
| **Domain** | [`Domain/Entities/Auditoria.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/Auditoria.cs) | Tabla de bitácora automática de cambios. |
| **Application** | [`Application/Interfaces/IUnitOfWork.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Application/Interfaces/IUnitOfWork.cs) | Interfaz unificada de acceso a repositorios y confirmación. |
| **Infrastructure** | [`Infrastructure/Data/AutoTallerDbContext.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Infrastructure/Data/AutoTallerDbContext.cs) | Configuración de EF Core, Seed Data y **Guardado con Auditoría**. |
| **Api** | [`Api/DTOs/`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/DTOs/) | Datos que viajan en las peticiones y respuestas HTTP. |
| **Api** | [`Api/Services/ClienteService.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Services/ClienteService.cs) | Registro atómico de Cliente + Vehículos en una transacción. |
| **Api** | [`Api/Services/OrdenServicioService.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Services/OrdenServicioService.cs) | Lógica de cálculo de precios, aprobación y descuento de inventario. |
| **Api** | [`Api/Controllers/`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Controllers/) | Controladores de entrada (Swagger/Endpoints) y filtros de rol JWT. |
| **Api** | [`Api/Program.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Program.cs) | Configuración de servicios, base de datos MySQL, JWT y auto-migración. |

---

## 🛠️ 2. ENTIDADES DE DOMINIO: ANÁLISIS DETALLADO

Las entidades de dominio contienen las propiedades físicas de las tablas de la base de datos y métodos que ejecutan **lógica de negocio interna** (encapsulamiento).

### A. Repuesto.cs
```csharp
public class Repuesto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int CantidadStock { get; set; }
    public decimal PrecioUnitario { get; set; }
    public int? ProveedorId { get; set; } // FK Normalizada
    public Proveedor? Proveedor { get; set; }

    // Método de Negocio: Incrementa stock en compras
    public void AumentarStock(int cantidad)
    {
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a cero");
        CantidadStock += cantidad;
    }

    // Método de Negocio: Reduce stock en reparaciones
    public void ReducirStock(int cantidad)
    {
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a cero");
        if (CantidadStock < cantidad) 
            throw new InvalidOperationException($"Stock insuficiente para '{Descripcion}'. Disponible: {CantidadStock}");
        CantidadStock -= cantidad;
    }
}
```

### B. OrdenServicio.cs
Representa el estado del vehículo en el taller y controla la progresión de estados (`Ingresada`, `Aprobada`, `EnReparacion`, `ListaParaEntrega`, `Cerrada`, `Cancelada`).
```csharp
public class OrdenServicio
{
    public int Id { get; set; }
    public int VehiculoId { get; set; }
    public Vehiculo? Vehiculo { get; set; }
    public int ServicioId { get; set; } // FK del Catálogo
    public CatalogoServicio? Servicio { get; set; }
    public EstadoOrden Estado { get; set; } = EstadoOrden.Ingresada;
    public int? MecanicoId { get; set; }
    public Usuario? Mecanico { get; set; }
    public int KilometrajeIngreso { get; set; }
    public DateTime FechaIngreso { get; set; } = DateTime.UtcNow;
    public DateTime? FechaEstimadaEntrega { get; set; }
    public bool AprobadaPorCliente { get; set; } = false; // Requiere aprobación previa

    // Métodos de Negocio:
    public void AsignarMecanico(int mecanicoId)
    {
        if (Estado == EstadoOrden.Cerrada || Estado == EstadoOrden.Cancelada)
            throw new InvalidOperationException("No se puede asignar mecánico a una orden cerrada o cancelada.");
        MecanicoId = mecanicoId;
    }

    public void CalcularFechaEstimadaEntrega()
    {
        // Regla: 2 días estimados desde el ingreso
        FechaEstimadaEntrega = FechaIngreso.AddDays(2);
    }

    public void AprobarOrden()
    {
        if (Estado != EstadoOrden.Ingresada)
            throw new InvalidOperationException("La orden solo se puede aprobar si está en estado Ingresada.");
        AprobadaPorCliente = true;
        Estado = EstadoOrden.Aprobada;
    }

    public void CambiarEstado(EstadoOrden nuevoEstado)
    {
        // Reglas de transición de estados
        if (Estado == EstadoOrden.Cerrada)
            throw new InvalidOperationException("No se puede modificar una orden que ya está Cerrada.");

        if (nuevoEstado == EstadoOrden.EnReparacion && !AprobadaPorCliente)
            throw new InvalidOperationException("No se puede iniciar reparación si el cliente no ha aprobado la orden.");

        Estado = nuevoEstado;
    }
}
```

### C. DetalleOrden.cs
Controla el cálculo matemático del subtotal de repuestos.
```csharp
public class DetalleOrden
{
    public int Id { get; set; }
    public int OrdenServicioId { get; set; }
    public OrdenServicio? OrdenServicio { get; set; }
    public int RepuestoId { get; set; }
    public Repuesto? Repuesto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    
    // Propiedad calculada
    public decimal Subtotal => Cantidad * PrecioUnitario;
}
```

---

## 💾 3. CONFIGURACIÓN E INFRAESTRUCTURA DE PERSISTENCIA

Esta capa administra la base de datos MySQL mediante Entity Framework Core. El archivo más crítico aquí es `AutoTallerDbContext.cs`.

### Auditoría Automática en Guardados (Interceptando SaveChangesAsync)
El método `SaveChangesAsync` en `AutoTallerDbContext` intercepta todos los cambios que EF Core va a enviar a la base de datos y crea un registro histórico en la tabla `Auditorias` de manera automática y transparente para los servicios.
```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var auditEntries = new List<Auditoria>();

    // 1. Analizar el rastreador de cambios (ChangeTracker) de EF Core
    foreach (var entry in ChangeTracker.Entries())
    {
        // Ignorar la propia tabla de auditoría para evitar bucles infinitos
        if (entry.Entity is Auditoria || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
            continue;

        var audit = new Auditoria
        {
            EntidadAfectada = entry.Entity.GetType().Name,
            FechaAccion = DateTime.UtcNow,
            Accion = entry.State.ToString() // "Added", "Modified", "Deleted"
        };

        // Capturar datos específicos según el tipo de acción
        if (entry.State == EntityState.Added)
        {
            audit.Detalles = "Registro Creado.";
        }
        else if (entry.State == EntityState.Modified)
        {
            var modifiedProps = entry.Properties
                .Where(p => p.IsModified)
                .Select(p => $"{p.Metadata.Name}: '{p.OriginalValue}' ➔ '{p.CurrentValue}'");
            audit.Detalles = "Propiedades modificadas: " + string.Join(", ", modifiedProps);
        }
        else if (entry.State == EntityState.Deleted)
        {
            audit.Detalles = "Registro eliminado físicamente.";
        }

        // Obtener el correo del usuario logueado mediante HttpContext
        var httpContext = _httpContextAccessor?.HttpContext;
        var userEmail = httpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        audit.UsuarioCorreo = userEmail ?? "Sistema/Semilla";

        auditEntries.Add(audit);
    }

    // 2. Si hay entradas registradas, agregarlas al DbContext
    if (auditEntries.Any())
    {
        await Auditorias.AddRangeAsync(auditEntries, cancellationToken);
    }

    // 3. Confirmar la transacción física en la base de datos
    return await base.SaveChangesAsync(cancellationToken);
}
```

---

## ⚡ 4. FLUJO DE SERVICIOS: LOGÍSICA TRANSACCIONAL Y CÁLCULOS

Los servicios en la capa `Api` orquestan los procesos transaccionales complejos y conectan los controladores con los repositorios.

### Registro Atómico (ClienteService.cs)
Garantiza que un cliente y sus vehículos se guarden de forma conjunta. Si el registro del auto falla (ej: VIN duplicado), la creación del cliente se cancela automáticamente (**Rollback**).
```csharp
public async Task<ClienteDto> RegistrarClienteConVehiculoAsync(CreateClienteDto dto)
{
    // 1. Mapear DTO a la entidad Cliente
    var cliente = _mapper.Map<Cliente>(dto);

    // 2. Agregar al rastreador (todavía no impacta la BD)
    await _unitOfWork.Clientes.AddAsync(cliente);

    // 3. Validar y agregar cada vehículo
    foreach (var vehiculoDto in dto.Vehiculos)
    {
        var existentes = await _unitOfWork.Vehiculos.FindAsync(v => v.Vin == vehiculoDto.Vin);
        if (existentes.Any())
        {
            // Provoca excepción: el flujo se detiene y no se guarda nada
            throw new InvalidOperationException($"Ya existe un vehículo con VIN '{vehiculoDto.Vin}'.");
        }

        var vehiculo = _mapper.Map<Vehiculo>(vehiculoDto);
        vehiculo.Cliente = cliente; // Enlace navegacional en memoria
        await _unitOfWork.Vehiculos.AddAsync(vehiculo);
    }

    // 4. Guardar todo en una sola transacción
    await _unitOfWork.CommitAsync();

    // 5. Devolver información mapeada
    var result = await _unitOfWork.Clientes.GetByIdAsync(cliente.Id);
    return _mapper.Map<ClienteDto>(result);
}
```

### Liquidación Matemática de Órdenes (OrdenServicioService.cs ➔ GenerarFacturaAsync)
Carga la orden, extrae el precio de la mano de obra del catálogo, suma el subtotal de repuestos y emite la factura final.
```csharp
public async Task<FacturaDto> GenerarFacturaAsync(int ordenId)
{
    // 1. Buscar orden de servicio
    var orden = await _unitOfWork.OrdenesServicio.GetByIdAsync(ordenId);
    if (orden == null) throw new Exception("Orden no encontrada");

    // 2. Obtener el servicio del catálogo para mano de obra dinámica
    var servicio = await _unitOfWork.CatalogoServicios.GetByIdAsync(orden.ServicioId);
    if (servicio == null) throw new Exception("Catálogo de servicio no encontrado");

    // 3. Sumar el costo de todos los repuestos asociados
    var detalles = await _unitOfWork.DetallesOrden.FindAsync(d => d.OrdenServicioId == ordenId);
    decimal totalRepuestos = detalles.Sum(d => d.Subtotal);

    // 4. Extraer mano de obra dinámica
    decimal totalManoObra = servicio.PrecioManoObra;

    // 5. Crear la factura y relacionarla
    var factura = new Factura
    {
        OrdenServicioId = ordenId,
        TotalRepuestos = totalRepuestos,
        TotalManoObra = totalManoObra,
        ResumenServicios = $"Servicio de {servicio.Nombre} para vehículo ID {orden.VehiculoId}"
    };

    await _unitOfWork.Facturas.AddAsync(factura);
    
    // 6. Cerrar automáticamente la orden para evitar modificaciones futuras
    orden.CambiarEstado(EstadoOrden.Cerrada);
    _unitOfWork.OrdenesServicio.Update(orden);

    // 7. Guardar cambios en la base de datos
    await _unitOfWork.CommitAsync();

    return _mapper.Map<FacturaDto>(factura);
}
```

---

## 🔒 5. CONTROLADORES Y AUTORIZACIÓN JWT

Los controladores actúan como el punto de entrada de las peticiones HTTP y aplican las políticas de seguridad basadas en roles definidos por JWT (`Admin`, `Mecanico`, `Recepcionista`).

### ClientesController.cs: Control de Rutas y Roles
```csharp
[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;
    private readonly IUnitOfWork _unitOfWork;

    // Constructor inyecta dependencias registradas en Program.cs
    public ClientesController(IClienteService clienteService, IUnitOfWork unitOfWork)
    {
        _clienteService = clienteService;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("registrar-con-vehiculo")]
    [Authorize(Roles = "Admin,Recepcionista")] // Bloquea accesos no autorizados
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
}
```

---

## 📝 6. MAPEO DE DTOs (AutoMapperProfile.cs)

`AutoMapper` mapea entidades complejas a DTOs limpios y planos para enviarlos por red.
```csharp
CreateMap<Vehiculo, VehiculoDto>()
    // Mapea el nombre de la Marca navegando: Vehiculo ➔ Modelo ➔ Marca ➔ Nombre
    .ForMember(dest => dest.MarcaNombre, opt => opt.MapFrom(src => src.Modelo != null && src.Modelo.Marca != null ? src.Modelo.Marca.Nombre : string.Empty))
    // Mapea el nombre del Modelo navegando: Vehiculo ➔ Modelo ➔ Nombre
    .ForMember(dest => dest.ModeloNombre, opt => opt.MapFrom(src => src.Modelo != null ? src.Modelo.Nombre : string.Empty))
    .ReverseMap();
```

---

## 🎓 7. CHULETA DE EXAMEN (CÓMO HACER CAMBIOS RÁPIDOS)

Usa esta guía rápida en tu examen si te piden añadir, modificar o depurar algo:

### 🌟 ESCENARIO A: Añadir una nueva propiedad a una Entidad (ej: "Añadir propiedad 'Color' a Vehiculo")
1. **Domain**: Abre [`Vehiculo.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/Vehiculo.cs) y añade la propiedad:
   ```csharp
   public string Color { get; set; } = string.Empty;
   ```
2. **DTOs**: Abre [`CreateVehiculoDto.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/DTOs/CreateVehiculoDto.cs) y [`VehiculoDto.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/DTOs/VehiculoDto.cs) y añade la misma propiedad.
3. **Database Config**: Si el campo debe ser requerido o tener longitud máxima, abre [`AutoTallerDbContext.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Infrastructure/Data/AutoTallerDbContext.cs) e indícalo en el `OnModelCreating`:
   ```csharp
   builder.Entity<Vehiculo>().Property(v => v.Color).HasMaxLength(30).IsRequired();
   ```
4. **Services/Controllers**: Si el mapeo se hace manual, edita el mapeador o controlador correspondiente para transferir la propiedad.
5. **Base de Datos**: Genera y aplica la migración en tu base de datos:
   ```bash
   dotnet ef migrations add AddColorToVehiculo --project Infrastructure --startup-project Api
   dotnet ef database update --project Infrastructure --startup-project Api
   ```

### 🌟 ESCENARIO B: Crear un nuevo Endpoint (ej: "Añadir endpoint para Cancelar una Orden de Servicio")
1. **Domain**: Abre [`OrdenServicio.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Domain/Entities/OrdenServicio.cs) y añade el método de negocio:
   ```csharp
   public void Cancelar()
   {
       if (Estado == EstadoOrden.Cerrada)
           throw new InvalidOperationException("No se puede cancelar una orden cerrada.");
       Estado = EstadoOrden.Cancelada;
   }
   ```
2. **Interfaces**: Abre [`IOrdenServicioService.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Interfaces/IOrdenServicioService.cs) y declara:
   ```csharp
   Task<bool> CancelarOrdenAsync(int ordenId);
   ```
3. **Services**: Abre [`OrdenServicioService.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Services/OrdenServicioService.cs) e impleméntalo:
   ```csharp
   public async Task<bool> CancelarOrdenAsync(int ordenId)
   {
       var orden = await _unitOfWork.OrdenesServicio.GetByIdAsync(ordenId);
       if (orden == null) return false;
       orden.Cancelar();
       _unitOfWork.OrdenesServicio.Update(orden);
       await _unitOfWork.CommitAsync();
       return true;
   }
   ```
4. **Controllers**: Abre [`OrdenesServicioController.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Controllers/OrdenesServicioController.cs) y publica el endpoint:
   ```csharp
   [HttpPut("{id}/cancelar")]
   [Authorize(Roles = "Admin,Recepcionista")]
   public async Task<IActionResult> Cancelar(int id)
   {
       var result = await _ordenServicioService.CancelarOrdenAsync(id);
       if (!result) return NotFound(new { Message = $"Orden con Id {id} no encontrada." });
       return Ok(new { Message = "Orden cancelada correctamente." });
   }
   ```

### 🌟 ESCENARIO C: Cambiar el precio de un Servicio o la lógica de Facturación
* Modifica la clase [`OrdenServicioService.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Services/OrdenServicioService.cs) en el método `GenerarFacturaAsync`.
* Si te piden aplicar un descuento del 10% si la orden tiene más de 3 repuestos:
  ```csharp
  decimal totalRepuestos = detalles.Sum(d => d.Subtotal);
  if (detalles.Count > 3)
  {
      totalRepuestos *= 0.90m; // 10% de descuento
  }
  ```

### 🌟 ESCENARIO D: Añadir un Filtro de Búsqueda (ej: "Listar vehículos por Año")
1. Abre [`VehiculosController.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Controllers/VehiculosController.cs).
2. Añade el parámetro al método `GetAll`:
   ```csharp
   [FromQuery] int? anio = null
   ```
3. Aplica el filtro LINQ antes de paginar:
   ```csharp
   if (anio.HasValue)
   {
       all = all.Where(v => v.Anio == anio.Value);
   }
   ```

---

## 🧭 8. LOCALIZACIÓN RÁPIDA DE PROBLEMAS (DEBUGGING)

| Error Común | Causa del Problema | Dónde Solucionarlo |
|---|---|---|
| **`401 Unauthorized`** | Token ausente, expirado o con firma JWT diferente. | Petición `POST /api/Usuarios/login` y verificar el header `Authorization: Bearer <token>` en Insomnia. |
| **`403 Forbidden`** | El rol del usuario logueado no tiene permisos para acceder al endpoint. | Comprobar los roles asignados en el atributo `[Authorize(Roles = "...")]` en el controlador correspondiente. |
| **`DbUpdateException` (FK Constraint)** | Intento de insertar una FK inexistente (ej: `ModeloId` que no está en la tabla `Modelos`) o borrar un registro con relaciones restrictivas activas. | Comprobar el Seed Data en [`AutoTallerDbContext.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Infrastructure/Data/AutoTallerDbContext.cs) o los objetos JSON que envías en el body. |
| **Propiedades en `null` en el JSON de salida** | Falta la regla en el perfil de mapeo de AutoMapper. | Configurar el mapeo `.ForMember(...)` en [`AutoMapperProfile.cs`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/Mappings/AutoMapperProfile.cs) para incluir las entidades navegacionales. |
| **Error al iniciar la base de datos** | El servidor local de MySQL está apagado o las credenciales son incorrectas. | Editar la cadena de conexión en [`appsettings.json`](file:///e:/Users/moggamex/Documents/moggamex/todo/learn/campuslands/Net/practica/ProyectoNet/Api/appsettings.json). |
