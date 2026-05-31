namespace Domain.Entities;

public class Vehiculo : BaseEntity
{
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string Vin { get; set; } = string.Empty;
    public int Kilometraje { get; set; }

    public int ClienteId { get; private set; }
    public Cliente? Cliente { get; set; }

    public ICollection<OrdenServicio> OrdenesServicio { get; set; } = new List<OrdenServicio>();

    // Lógica de Dominio: Cambiar propietario
    public void CambiarPropietario(int nuevoClienteId)
    {
        // Podríamos agregar validaciones extra aquí en el futuro
        ClienteId = nuevoClienteId;
    }

    // Inicializador / Constructor para EF o para crear
    public void AsignarCliente(int clienteId)
    {
        ClienteId = clienteId;
    }
}
