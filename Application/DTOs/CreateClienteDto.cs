namespace Application.DTOs;

public class CreateClienteDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;

    // Lista opcional de vehículos a asociar al momento del registro
    public ICollection<CreateVehiculoDto> Vehiculos { get; set; } = new List<CreateVehiculoDto>();
}
