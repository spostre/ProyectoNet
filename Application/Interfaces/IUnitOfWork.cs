using Domain.Entities;

namespace Application.Interfaces;

/// <summary>
/// Contrato del patrón Unit of Work. Define todos los repositorios disponibles y el método
/// para confirmar transacciones (CommitAsync).
/// 
/// Al inyectar IUnitOfWork en un servicio o controlador, se obtiene acceso a TODOS los repositorios
/// con el mismo DbContext, garantizando coherencia transaccional (todo se guarda o nada).
/// 
/// Para añadir una nueva entidad al sistema:
///  1. Crear la clase entidad en Domain/Entities.
///  2. Agregar el DbSet en AutoTallerDbContext.
///  3. Agregar la propiedad IGenericRepository aquí en IUnitOfWork.
///  4. Instanciar el repositorio en el constructor de UnitOfWork.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Cliente> Clientes { get; }
    IGenericRepository<Vehiculo> Vehiculos { get; }
    IGenericRepository<OrdenServicio> OrdenesServicio { get; }
    IGenericRepository<Repuesto> Repuestos { get; }
    IGenericRepository<DetalleOrden> DetallesOrden { get; }
    IGenericRepository<Factura> Facturas { get; }
    IGenericRepository<Usuario> Usuarios { get; }
    IGenericRepository<Auditoria> Auditorias { get; }
    IGenericRepository<Marca> Marcas { get; }
    IGenericRepository<Modelo> Modelos { get; }
    IGenericRepository<Proveedor> Proveedores { get; }
    IGenericRepository<CatalogoServicio> CatalogoServicios { get; }

    /// <summary>Persiste todos los cambios pendientes en la base de datos en una transacción.</summary>
    Task<int> CommitAsync();
}
