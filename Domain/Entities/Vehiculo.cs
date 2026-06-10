namespace Domain.Entities;

public class Vehiculo : BaseEntity
{
    /// <summary>
    /// Identificador del Modelo asociado al vehículo (Relación N:1 con Modelo).
    /// </summary>
    public int ModeloId { get; set; }
    
    /// <summary>
    /// Propiedad navegacional para acceder a los datos del Modelo y su Marca.
    /// </summary>
    public Modelo? Modelo { get; set; }
    
    /// <summary>
    /// Año de fabricación del vehículo.
    /// </summary>
    public int Anio { get; set; }
    
    /// <summary>
    /// Número de Identificación Vehicular (VIN). Debe ser único en el sistema.
    /// </summary>
    public string Vin { get; set; } = string.Empty;
    
    /// <summary>
    /// Kilometraje actual del vehículo en su último ingreso o actualización.
    /// </summary>
    public int Kilometraje { get; set; }

    /// <summary>
    /// Identificador del Cliente propietario del vehículo (Llave Foránea).
    /// </summary>
    public int ClienteId { get; private set; }
    
    /// <summary>
    /// Propiedad navegacional del Cliente propietario.
    /// </summary>
    public Cliente? Cliente { get; set; }

    /// <summary>
    /// Colección de Órdenes de Servicio registradas para este vehículo en el taller.
    /// </summary>
    public ICollection<OrdenServicio> OrdenesServicio { get; set; } = new List<OrdenServicio>();

    /// <summary>
    /// Lógica de Negocio: Cambia el propietario actual del vehículo a otro cliente.
    /// </summary>
    /// <param name="nuevoClienteId">Id del nuevo cliente propietario.</param>
    public void CambiarPropietario(int nuevoClienteId)
    {
        if (nuevoClienteId <= 0) 
            throw new ArgumentException("El Id del nuevo propietario debe ser un valor positivo.");
        ClienteId = nuevoClienteId;
    }

    /// <summary>
    /// Lógica de Negocio: Vincula o asigna el vehículo a un cliente al momento del registro.
    /// </summary>
    /// <param name="clienteId">Id del cliente a asociar.</param>
    public void AsignarCliente(int clienteId)
    {
        if (clienteId <= 0) 
            throw new ArgumentException("El Id del cliente debe ser un valor positivo.");
        ClienteId = clienteId;
    }
}
