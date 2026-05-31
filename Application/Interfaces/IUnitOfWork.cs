using Domain.Entities;

namespace Application.Interfaces;

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

    Task<int> CommitAsync();
}
