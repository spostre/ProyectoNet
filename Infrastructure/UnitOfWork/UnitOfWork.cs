using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories;

namespace Infrastructure.UnitOfWork;

/// <summary>
/// Implementación del patrón Unit of Work. Agrupa todos los repositorios disponibles
/// y expone un único punto de confirmación de transacciones (CommitAsync).
/// Al inyectar IUnitOfWork desde cualquier servicio o controlador, obtienes acceso
/// a todos los repositorios compartiendo el mismo DbContext, garantizando que todos
/// los cambios se guarden en una sola transacción atómica.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AutoTallerDbContext _context;

    // Repositorios expuestos: cada uno opera sobre su entidad de dominio usando el mismo DbContext
    public IGenericRepository<Cliente> Clientes { get; private set; }
    public IGenericRepository<Vehiculo> Vehiculos { get; private set; }
    public IGenericRepository<OrdenServicio> OrdenesServicio { get; private set; }
    public IGenericRepository<Repuesto> Repuestos { get; private set; }
    public IGenericRepository<DetalleOrden> DetallesOrden { get; private set; }
    public IGenericRepository<Factura> Facturas { get; private set; }
    public IGenericRepository<Usuario> Usuarios { get; private set; }
    public IGenericRepository<Auditoria> Auditorias { get; private set; }
    public IGenericRepository<Marca> Marcas { get; private set; }
    public IGenericRepository<Modelo> Modelos { get; private set; }
    public IGenericRepository<Proveedor> Proveedores { get; private set; }
    public IGenericRepository<CatalogoServicio> CatalogoServicios { get; private set; }

    /// <summary>
    /// Constructor que recibe el DbContext inyectado y crea instancias de cada repositorio genérico.
    /// Todos comparten la misma instancia del contexto para mantener coherencia transaccional.
    /// </summary>
    public UnitOfWork(AutoTallerDbContext context)
    {
        _context = context;
        Clientes = new GenericRepository<Cliente>(_context);
        Vehiculos = new GenericRepository<Vehiculo>(_context);
        OrdenesServicio = new GenericRepository<OrdenServicio>(_context);
        Repuestos = new GenericRepository<Repuesto>(_context);
        DetallesOrden = new GenericRepository<DetalleOrden>(_context);
        Facturas = new GenericRepository<Factura>(_context);
        Usuarios = new GenericRepository<Usuario>(_context);
        Auditorias = new GenericRepository<Auditoria>(_context);
        Marcas = new GenericRepository<Marca>(_context);
        Modelos = new GenericRepository<Modelo>(_context);
        Proveedores = new GenericRepository<Proveedor>(_context);
        CatalogoServicios = new GenericRepository<CatalogoServicio>(_context);
    }

    /// <summary>
    /// Confirma todos los cambios pendientes en el contexto y los persiste en MySQL.
    /// Internamente llama a SaveChangesAsync del DbContext, el cual también lanza el interceptor de Auditoría.
    /// </summary>
    public async Task<int> CommitAsync()
    {
        return await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Libera los recursos del DbContext al finalizar el ciclo de vida del servicio (Scoped en DI).
    /// </summary>
    public void Dispose()
    {
        _context.Dispose();
    }
}
