namespace Api.DTOs;

/// <summary>
/// DTO de entrada para crear un nuevo Cliente.
/// Se envía en el body del POST /api/clientes/registrar-con-vehiculo.
/// La lista de Vehiculos puede estar vacía si se quiere registrar el cliente sin vehículos,
/// pero normalmente se registra al menos un vehículo junto con el cliente.
/// </summary>
public class CreateClienteDto
{
    /// <summary>Nombre completo del cliente.</summary>
    public string Nombre { get; set; } = string.Empty;
    /// <summary>Teléfono de contacto.</summary>
    public string Telefono { get; set; } = string.Empty;
    /// <summary>Correo electrónico del cliente.</summary>
    public string Correo { get; set; } = string.Empty;

    // Lista opcional de vehículos a asociar al momento del registro
    /// <summary>Vehículos a registrar para este cliente en la misma operación (puede ser lista vacía).</summary>
    public ICollection<CreateVehiculoDto> Vehiculos { get; set; } = new List<CreateVehiculoDto>();
}
