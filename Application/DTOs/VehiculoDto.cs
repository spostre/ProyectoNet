namespace Application.DTOs;

public class VehiculoDto
{
    public int Id { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string Vin { get; set; } = string.Empty;
    public int Kilometraje { get; set; }
    public int ClienteId { get; set; }
}
