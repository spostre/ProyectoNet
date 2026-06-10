using System.Linq.Expressions;

namespace Application.Interfaces;

/// <summary>
/// Contrato que define las operaciones CRUD básicas para cualquier entidad T.
/// Al depender de esta interfaz (y no de la implementación concreta), los servicios
/// y controladores quedan desacoplados de Entity Framework Core.
/// Si algún día se cambia el ORM, solo se necesita reemplazar GenericRepository.cs.
/// </summary>
/// <typeparam name="T">Tipo de entidad de dominio.</typeparam>
public interface IGenericRepository<T> where T : class
{
    /// <summary>Busca por clave primaria. Retorna null si no existe.</summary>
    Task<T?> GetByIdAsync(int id);
    /// <summary>Retorna todos los registros de la tabla. Usar con cuidado en tablas grandes.</summary>
    Task<IEnumerable<T>> GetAllAsync();
    /// <summary>Filtra registros con una expresión lambda (equivale a WHERE en SQL).</summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);
    /// <summary>Agrega una entidad al contexto (el INSERT ocurre en CommitAsync).</summary>
    Task AddAsync(T entity);
    /// <summary>Marca la entidad como modificada (el UPDATE ocurre en CommitAsync).</summary>
    void Update(T entity);
    /// <summary>Marca la entidad para eliminación (el DELETE ocurre en CommitAsync).</summary>
    void Remove(T entity);
}
