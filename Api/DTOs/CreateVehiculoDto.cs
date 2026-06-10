namespace Api.DTOs;

public class CreateVehiculoDto
{
    public int ModeloId { get; set; }
    public int Anio { get; set; }
    public string Vin { get; set; } = string.Empty;
    public int Kilometraje { get; set; }
}
