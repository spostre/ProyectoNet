namespace Domain.Entities;

/// <summary>
/// Entidad que representa una Marca de vehículos (ej. Toyota, Honda, Ford).
/// Una Marca puede tener muchos Modelos (relación 1:N).
/// La jerarquía es: Marca → Modelo → Vehículo.
/// </summary>
public class Marca : BaseEntity
{
    /// <summary>Nombre de la marca (ej. "Toyota", "Chevrolet").</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Modelos que pertenecen a esta marca (relación 1:N).</summary>
    public ICollection<Modelo> Modelos { get; set; } = new List<Modelo>();
}
