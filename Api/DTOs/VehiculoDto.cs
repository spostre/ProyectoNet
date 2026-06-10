namespace Api.DTOs;

public class VehiculoDto
{
    public int Id { get; set; }
    public int ModeloId { get; set; }
    public string MarcaNombre { get; set; } = string.Empty;
    public string ModeloNombre { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string Vin { get; set; } = string.Empty;
    public int Kilometraje { get; set; }
    public int ClienteId { get; set; }
}
