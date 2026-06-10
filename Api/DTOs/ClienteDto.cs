namespace Api.DTOs;

public class ClienteDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public ICollection<VehiculoDto> Vehiculos { get; set; } = new List<VehiculoDto>();
}
