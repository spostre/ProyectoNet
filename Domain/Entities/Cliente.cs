namespace Domain.Entities;

public class Cliente : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;

    // Relación opcional/obligatoria con el sistema de usuarios para login
    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
