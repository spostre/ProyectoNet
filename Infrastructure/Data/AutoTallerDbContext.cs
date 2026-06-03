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
    public DbSet<Vehiculo> Vehiculos { get; set; }
    public DbSet<OrdenServicio> OrdenesServicio { get; set; }
    public DbSet<Repuesto> Repuestos { get; set; }
    public DbSet<DetalleOrden> DetallesOrden { get; set; }
    public DbSet<Factura> Facturas { get; set; }
    public DbSet<Auditoria> Auditorias { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Correo).HasMaxLength(100);
            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.SetNull); // Si se borra el usuario, el cliente queda huérfano de login pero no se borra.
        });

        // Vehiculo
        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.Property(e => e.Vin).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Marca).HasMaxLength(50);
            entity.Property(e => e.Modelo).HasMaxLength(50);

            entity.HasOne(e => e.Cliente)
                  .WithMany(c => c.Vehiculos)
                  .HasForeignKey(e => e.ClienteId)
                  .OnDelete(DeleteBehavior.Restrict); // No borrar cliente si tiene vehículos activos
        });

        // Repuesto
        modelBuilder.Entity<Repuesto>(entity =>
        {
            entity.HasIndex(e => e.Codigo).IsUnique();
            entity.Property(e => e.Codigo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(200);
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,2)");
        });

        // OrdenServicio
        modelBuilder.Entity<OrdenServicio>(entity =>
        {
            entity.HasOne(e => e.Vehiculo)
                  .WithMany(v => v.OrdenesServicio)
                  .HasForeignKey(e => e.VehiculoId)
                  .OnDelete(DeleteBehavior.Restrict); // No borrar vehículo si tiene ordenes

            entity.HasOne(e => e.Mecanico)
                  .WithMany()
                  .HasForeignKey(e => e.MecanicoId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // DetalleOrden
        modelBuilder.Entity<DetalleOrden>(entity =>
        {
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.OrdenServicio)
                  .WithMany(o => o.Detalles)
                  .HasForeignKey(e => e.OrdenServicioId)
                  .OnDelete(DeleteBehavior.Cascade); // Borrar orden borra detalles

            entity.HasOne(e => e.Repuesto)
                  .WithMany()
                  .HasForeignKey(e => e.RepuestoId)
                  .OnDelete(DeleteBehavior.Restrict); // No borrar repuesto si está en una orden
        });

        // Factura
        modelBuilder.Entity<Factura>(entity =>
        {
            entity.Property(e => e.TotalRepuestos).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalManoObra).HasColumnType("decimal(18,2)");
            
            entity.HasOne(e => e.OrdenServicio)
                  .WithMany()
                  .HasForeignKey(e => e.OrdenServicioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed Data
        modelBuilder.Entity<Usuario>().HasData(
            new { Id = 1, Correo = "admin@taller.com", PasswordHash = "admin123", Rol = RolUsuario.Admin },
            new { Id = 2, Correo = "mecanico@taller.com", PasswordHash = "mecanico123", Rol = RolUsuario.Mecanico }
        );

        modelBuilder.Entity<Cliente>().HasData(
            new { Id = 1, Nombre = "Juan Perez", Telefono = "555-1234", Correo = "juan@perez.com" }
        );

        modelBuilder.Entity<Vehiculo>().HasData(
            new { Id = 1, Marca = "Toyota", Modelo = "Corolla", Anio = 2020, Vin = "VIN1234567890COROLLA", Kilometraje = 45000, ClienteId = 1 }
        );

        modelBuilder.Entity<Repuesto>().HasData(
            new { Id = 1, Codigo = "FILT-ACEITE", Descripcion = "Filtro de Aceite sintético", CantidadStock = 50, PrecioUnitario = 15.50m },
            new { Id = 2, Codigo = "PAST-FREN-DEL", Descripcion = "Pastillas de freno delanteras", CantidadStock = 20, PrecioUnitario = 45.00m },
            new { Id = 3, Codigo = "BUJIA-PLAT", Descripcion = "Bujía de platino de alto rendimiento", CantidadStock = 100, PrecioUnitario = 8.20m }
        );
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is not Auditoria && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            .ToList();

        if (entries.Any())
        {
            // Obtener datos del usuario logueado en la petición HTTP actual
            int? usuarioId = null;
            string usuarioCorreo = "Sistema";

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

            foreach (var entry in entries)
            {
                var action = entry.State.ToString();
                var entityName = entry.Entity.GetType().Name;

                // Evitar nombres proxy dinámicos generados por EF Core
                if (entityName.Contains("Proxy"))
                {
                    entityName = entry.Entity.GetType().BaseType?.Name ?? entityName;
                }

                var details = $"Entidad {entityName} con estado {action}. ";
                if (entry.State == EntityState.Modified)
                {
                    var modifiedProps = entry.Properties.Where(p => p.IsModified);
                    details += "Propiedades modificadas: " + string.Join(", ", modifiedProps.Select(p => $"{p.Metadata.Name} (Original: {p.OriginalValue} -> Nuevo: {p.CurrentValue})"));
                }
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

            await Auditorias.AddRangeAsync(auditRecords, cancellationToken);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
