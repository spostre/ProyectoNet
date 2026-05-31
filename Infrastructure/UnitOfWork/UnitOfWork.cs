using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories;

namespace Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AutoTallerDbContext _context;

    public IGenericRepository<Cliente> Clientes { get; private set; }
    public IGenericRepository<Vehiculo> Vehiculos { get; private set; }
    public IGenericRepository<OrdenServicio> OrdenesServicio { get; private set; }
    public IGenericRepository<Repuesto> Repuestos { get; private set; }
    public IGenericRepository<DetalleOrden> DetallesOrden { get; private set; }
    public IGenericRepository<Factura> Facturas { get; private set; }
    public IGenericRepository<Usuario> Usuarios { get; private set; }
    public IGenericRepository<Auditoria> Auditorias { get; private set; }

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
    }

    public async Task<int> CommitAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
