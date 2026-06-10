namespace Domain.Entities;

/// <summary>
/// Entidad que representa un Modelo de vehículo (ej. Corolla, Civic, Ranger).
/// Un Modelo pertenece a una Marca (relación N:1) y puede tener muchos Vehículos (relación 1:N).
/// La jerarquía completa es: Marca → Modelo → Vehículo → OrdenServicio.
/// </summary>
public class Modelo : BaseEntity
{
    /// <summary>Nombre del modelo (ej. "Corolla", "Civic").</summary>
    public string Nombre { get; set; } = string.Empty;
    /// <summary>FK a la Marca a la que pertenece este modelo.</summary>
    public int MarcaId { get; set; }
    /// <summary>Navegación hacia la Marca (cargada con Include() en EF Core si se necesita).</summary>
    public Marca? Marca { get; set; }

    /// <summary>Vehículos registrados con este modelo (relación 1:N).</summary>
    public ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
