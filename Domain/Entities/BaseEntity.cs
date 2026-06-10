namespace Domain.Entities;

/// <summary>
/// Clase base abstracta para todas las entidades del dominio.
/// Provee la propiedad Id (clave primaria) que EF Core mapea automáticamente a la columna "Id" (INT AUTO_INCREMENT) en MySQL.
/// Todas las entidades heredan de esta clase para evitar repetir la declaración de Id.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Clave primaria de la entidad. EF Core la genera automáticamente (IDENTITY/AUTO_INCREMENT).</summary>
    public int Id { get; set; }
}
