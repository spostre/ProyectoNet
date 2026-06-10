using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories;

/// <summary>
/// Repositorio genérico reutilizable para cualquier entidad T.
/// Implementa las operaciones CRUD básicas (Create, Read, Update, Delete) usando Entity Framework Core.
/// Al ser genérico, evita repetir el mismo código de acceso a datos para cada tabla.
/// Para añadir un nuevo repositorio en el proyecto, basta con agregar la propiedad en IUnitOfWork
/// y en UnitOfWork, usando este mismo GenericRepository.
/// </summary>
/// <typeparam name="T">Tipo de entidad del dominio (debe ser una clase).</typeparam>
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AutoTallerDbContext _context; // Contexto de EF Core compartido
    internal DbSet<T> dbSet; // Referencia al DbSet de la entidad T (equivalente a la tabla en BD)

    /// <summary>
    /// Constructor: recibe el contexto y obtiene el DbSet correspondiente a la entidad T.
    /// context.Set&lt;T&gt;() resuelve dinámicamente cuál tabla gestionar.
    /// </summary>
    public GenericRepository(AutoTallerDbContext context)
    {
        _context = context;
        this.dbSet = context.Set<T>(); // Equivale a _context.Clientes, _context.Vehiculos, etc.
    }

    /// <summary>
    /// Busca una entidad por su clave primaria (Id).
    /// FindAsync es optimizado: primero busca en el cache del contexto, luego va a la BD.
    /// </summary>
    public async Task<T?> GetByIdAsync(int id)
    {
        return await dbSet.FindAsync(id);
    }

    /// <summary>
    /// Obtiene todas las filas de la tabla como una lista en memoria.
    /// Nota: carga todo el conjunto de datos; usa paginación en el controlador para datasets grandes.
    /// </summary>
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await dbSet.ToListAsync();
    }

    /// <summary>
    /// Filtra entidades mediante una expresión lambda (equivale a un WHERE en SQL).
    /// Ejemplo de uso: FindAsync(v => v.ClienteId == 5) → trae los vehículos del cliente 5.
    /// </summary>
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression)
    {
        return await dbSet.Where(expression).ToListAsync();
    }

    /// <summary>
    /// Marca la entidad para ser INSERTADA en la BD. El INSERT real ocurre al llamar CommitAsync().
    /// </summary>
    public async Task AddAsync(T entity)
    {
        await dbSet.AddAsync(entity);
    }

    /// <summary>
    /// Marca la entidad para ser ACTUALIZADA en la BD.
    /// Attach la registra en el contexto si no lo está, y Entry().State = Modified fuerza un UPDATE de todos sus campos.
    /// </summary>
    public void Update(T entity)
    {
        dbSet.Attach(entity);                                    // Adjunta la entidad al tracker del contexto
        _context.Entry(entity).State = EntityState.Modified;    // Marca todos los campos como "sucios" para UPDATE
    }

    /// <summary>
    /// Marca la entidad para ser ELIMINADA de la BD. El DELETE ocurre al llamar CommitAsync().
    /// Si la entidad no está siendo rastreada (Detached), primero la adjunta para que EF pueda eliminarla.
    /// </summary>
    public void Remove(T entity)
    {
        // Si la entidad no está rastreada por el contexto, adjuntarla primero
        if (_context.Entry(entity).State == EntityState.Detached)
        {
            dbSet.Attach(entity);
        }
        dbSet.Remove(entity); // Marca para DELETE en el próximo CommitAsync()
    }
}
