namespace Domain.Entities;

/// <summary>
/// Entidad que representa a un Cliente del taller.
/// Un cliente puede tener uno o más vehículos, y cada vehículo puede tener órdenes de servicio.
/// Opcionalmente puede tener vinculado un Usuario del sistema (para login del cliente).
/// </summary>
public class Cliente : BaseEntity
{
    /// <summary>Nombre completo del cliente.</summary>
    public string Nombre { get; set; } = string.Empty;
    /// <summary>Teléfono de contacto del cliente.</summary>
    public string Telefono { get; set; } = string.Empty;
    /// <summary>Correo electrónico del cliente.</summary>
    public string Correo { get; set; } = string.Empty;

    // Relación opcional/obligatoria con el sistema de usuarios para login
    /// <summary>FK opcional hacia un Usuario del sistema (para portal de clientes).</summary>
    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    /// <summary>Colección de vehículos registrados para este cliente (relación 1:N).</summary>
    public ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
