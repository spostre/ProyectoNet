namespace Application.DTOs;

public class CreateOrdenServicioDto
{
    public int VehiculoId { get; set; }
    public string TipoServicio { get; set; } = string.Empty;
    public int? MecanicoId { get; set; }
}
