using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Data;

public class AutoTallerDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AutoTallerDbContext(DbContextOptions<AutoTallerDbContext> options, IHttpContextAccessor? httpContextAccessor = null) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Marca> Marcas { get; set; }
    public DbSet<Modelo> Modelos { get; set; }
    public DbSet<Vehiculo> Vehiculos { get; set; }
    public DbSet<CatalogoServicio> CatalogoServicios { get; set; }
    public DbSet<OrdenServicio> OrdenesServicio { get; set; }
    public DbSet<Proveedor> Proveedores { get; set; }
    public DbSet<Repuesto> Repuestos { get; set; }
    public DbSet<DetalleOrden> DetallesOrden { get; set; }
    public DbSet<Factura> Facturas { get; set; }
    public DbSet<Auditoria> Auditorias { get; set; }

    /// <summary>
    /// Método de configuración del modelo relacional.
    /// Define restricciones de tablas, llaves primarias/foráneas, índices únicos y comportamiento de borrado en cascada.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cliente: Nombre es obligatorio (max 100), correo es opcional.
        // Si el Usuario asociado al cliente se elimina, el campo UsuarioId del Cliente se vuelve nulo (SetNull).
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Correo).HasMaxLength(100);
            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.SetNull); 
        });

        // Marca: El nombre es obligatorio y debe ser único en la BD.
        modelBuilder.Entity<Marca>(entity =>
        {
            entity.Property(e => e.Nombre).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Nombre).IsUnique();
        });

        // Modelo: Nombre es obligatorio. Se relaciona con Marca.
        // Al borrar una Marca, NO se borran en cascada sus modelos (Restrict).
        modelBuilder.Entity<Modelo>(entity =>
        {
            entity.Property(e => e.Nombre).HasMaxLength(50).IsRequired();
            entity.HasOne(e => e.Marca)
                  .WithMany(m => m.Modelos)
                  .HasForeignKey(e => e.MarcaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Vehiculo: VIN es obligatorio y debe ser único.
        // Relaciones restrictivas con Cliente y Modelo para evitar borrado accidental de vehículos.
        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.Property(e => e.Vin).HasMaxLength(50).IsRequired();

            entity.HasOne(e => e.Modelo)
                  .WithMany(m => m.Vehiculos)
                  .HasForeignKey(e => e.ModeloId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Cliente)
                  .WithMany(c => c.Vehiculos)
                  .HasForeignKey(e => e.ClienteId)
                  .OnDelete(DeleteBehavior.Restrict); 
        });

        // Proveedor: Gestión de contactos del proveedor.
        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.Property(e => e.NombreEmpresa).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Contacto).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.Correo).HasMaxLength(100);
        });

        // Repuesto: Código de repuesto único en el inventario.
        // El precio unitario se define con 18 dígitos y 2 decimales.
        // Si el Proveedor se borra, el Repuesto mantiene su registro marcando el campo ProveedorId como nulo (SetNull).
        modelBuilder.Entity<Repuesto>(entity =>
        {
            entity.HasIndex(e => e.Codigo).IsUnique();
            entity.Property(e => e.Codigo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(200);
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,2)");
            
            entity.HasOne(e => e.Proveedor)
                  .WithMany(p => p.Repuestos)
                  .HasForeignKey(e => e.ProveedorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // CatalogoServicio: Nombre y precio del servicio de taller.
        modelBuilder.Entity<CatalogoServicio>(entity =>
        {
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PrecioManoObra).HasColumnType("decimal(18,2)");
        });

        // OrdenServicio: Relaciones restrictivas de protección.
        modelBuilder.Entity<OrdenServicio>(entity =>
        {
            entity.HasOne(e => e.Vehiculo)
                  .WithMany(v => v.OrdenesServicio)
                  .HasForeignKey(e => e.VehiculoId)
                  .OnDelete(DeleteBehavior.Restrict); 

            entity.HasOne(e => e.Mecanico)
                  .WithMany()
                  .HasForeignKey(e => e.MecanicoId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Servicio)
                  .WithMany(s => s.OrdenesServicio)
                  .HasForeignKey(e => e.ServicioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // DetalleOrden: Si se borra la Orden de Servicio principal, sus registros de repuestos usados se borran en cascada (Cascade).
        modelBuilder.Entity<DetalleOrden>(entity =>
        {
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.OrdenServicio)
                  .WithMany(o => o.Detalles)
                  .HasForeignKey(e => e.OrdenServicioId)
                  .OnDelete(DeleteBehavior.Cascade); 

            entity.HasOne(e => e.Repuesto)
                  .WithMany()
                  .HasForeignKey(e => e.RepuestoId)
                  .OnDelete(DeleteBehavior.Restrict); 
        });

        // Factura: Comprobante. Precio decimal (18,2).
        modelBuilder.Entity<Factura>(entity =>
        {
            entity.Property(e => e.TotalRepuestos).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalManoObra).HasColumnType("decimal(18,2)");
            
            entity.HasOne(e => e.OrdenServicio)
                  .WithMany()
                  .HasForeignKey(e => e.OrdenServicioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed Data (Datos semilla precargados en la base de datos)
        modelBuilder.Entity<Usuario>().HasData(
            new { Id = 1, Correo = "admin@taller.com", PasswordHash = "admin123", Rol = RolUsuario.Admin },
            new { Id = 2, Correo = "mecanico@taller.com", PasswordHash = "mecanico123", Rol = RolUsuario.Mecanico },
            new { Id = 3, Correo = "recepcionista@taller.com", PasswordHash = "recepcionista123", Rol = RolUsuario.Recepcionista }
        );

        modelBuilder.Entity<Cliente>().HasData(
            new { Id = 1, Nombre = "Juan Perez", Telefono = "555-1234", Correo = "juan@perez.com" }
        );

        modelBuilder.Entity<Marca>().HasData(
            new { Id = 1, Nombre = "Toyota" },
            new { Id = 2, Nombre = "Honda" },
            new { Id = 3, Nombre = "Ford" }
        );

        modelBuilder.Entity<Modelo>().HasData(
            new { Id = 1, Nombre = "Corolla", MarcaId = 1 },
            new { Id = 2, Nombre = "Hilux", MarcaId = 1 },
            new { Id = 3, Nombre = "Civic", MarcaId = 2 },
            new { Id = 4, Nombre = "Fiesta", MarcaId = 3 }
        );

        modelBuilder.Entity<Vehiculo>().HasData(
            new { Id = 1, ModeloId = 1, Anio = 2020, Vin = "VIN1234567890COROLLA", Kilometraje = 45000, ClienteId = 1 }
        );

        modelBuilder.Entity<Proveedor>().HasData(
            new { Id = 1, NombreEmpresa = "Autopartes Global S.A.", Contacto = "Carlos Ruiz", Telefono = "111-2222", Correo = "ventas@autoglobal.com" },
            new { Id = 2, NombreEmpresa = "Distribuidora El Motor", Contacto = "Ana Gomez", Telefono = "333-4444", Correo = "contacto@elmotor.com" }
        );

        modelBuilder.Entity<Repuesto>().HasData(
            new { Id = 1, Codigo = "FILT-ACEITE", Descripcion = "Filtro de Aceite sintético", CantidadStock = 50, PrecioUnitario = 15.50m, ProveedorId = 1 },
            new { Id = 2, Codigo = "PAST-FREN-DEL", Descripcion = "Pastillas de freno delanteras", CantidadStock = 20, PrecioUnitario = 45.00m, ProveedorId = 2 },
            new { Id = 3, Codigo = "BUJIA-PLAT", Descripcion = "Bujía de platino de alto rendimiento", CantidadStock = 100, PrecioUnitario = 8.20m, ProveedorId = 1 }
        );

        modelBuilder.Entity<CatalogoServicio>().HasData(
            new { Id = 1, Nombre = "Diagnóstico Computarizado", Descripcion = "Escaneo con herramienta OBD2", PrecioManoObra = 50.00m },
            new { Id = 2, Nombre = "Mantenimiento Preventivo", Descripcion = "Cambio de aceite, filtros y revisión general", PrecioManoObra = 80.00m },
            new { Id = 3, Nombre = "Alineación y Balanceo", Descripcion = "Alineación 3D y balanceo de 4 ruedas", PrecioManoObra = 60.00m },
            new { Id = 4, Nombre = "Reparación General", Descripcion = "Mano de obra para reparaciones complejas", PrecioManoObra = 150.00m }
        );
    }

    /// <summary>
    /// Intercepta la grabación física de los cambios en BD para generar de forma transaccional y automática
    /// un registro en la tabla Auditorias con el usuario ejecutor, acción y detalles.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Obtener todas las entidades agregadas, modificadas o eliminadas que requieran auditoría
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is not Auditoria && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            .ToList();

        if (entries.Any())
        {
            int? usuarioId = null;
            string usuarioCorreo = "Sistema";

            // 2. Extraer del HttpContext el usuario autenticado (extrae claims del token JWT)
            var user = _httpContextAccessor?.HttpContext?.User;
            if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
            {
                var subClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("sub");
                if (subClaim != null && int.TryParse(subClaim.Value, out var parsedId))
                {
                    usuarioId = parsedId;
                }

                var emailClaim = user.FindFirst(ClaimTypes.Email) ?? user.FindFirst("email");
                if (emailClaim != null)
                {
                    usuarioCorreo = emailClaim.Value;
                }
            }

            var auditRecords = new List<Auditoria>();

            // 3. Crear un registro de auditoría por cada entidad modificada
            foreach (var entry in entries)
            {
                var action = entry.State.ToString();
                var entityName = entry.Entity.GetType().Name;

                // Quitar proxies de lazy loading si existen
                if (entityName.Contains("Proxy"))
                {
                    entityName = entry.Entity.GetType().BaseType?.Name ?? entityName;
                }

                var details = $"Entidad {entityName} con estado {action}. ";
                
                // Si es modificación, registrar detalladamente qué valor cambió a qué valor
                if (entry.State == EntityState.Modified)
                {
                    var modifiedProps = entry.Properties.Where(p => p.IsModified);
                    details += "Propiedades modificadas: " + string.Join(", ", modifiedProps.Select(p => $"{p.Metadata.Name} (Original: {p.OriginalValue} -> Nuevo: {p.CurrentValue})"));
                }
                // Si es inserción, registrar todos los valores iniciales creados
                else if (entry.State == EntityState.Added)
                {
                    details += "Nuevos valores: " + string.Join(", ", entry.Properties.Select(p => $"{p.Metadata.Name}: {p.CurrentValue}"));
                }

                auditRecords.Add(new Auditoria
                {
                    EntidadAfectada = entityName,
                    Accion = action,
                    UsuarioId = usuarioId,
                    UsuarioCorreo = usuarioCorreo,
                    FechaAccion = DateTime.UtcNow,
                    Detalles = details
                });
            }

            // 4. Agregar los registros históricos al contexto antes de guardar
            await Auditorias.AddRangeAsync(auditRecords, cancellationToken);
        }

        // 5. Proceder con el guardado normal
        return await base.SaveChangesAsync(cancellationToken);
    }
}
