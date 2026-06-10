namespace Api.DTOs;

public class CreateOrdenServicioDto
{
    public int VehiculoId { get; set; }
    public int ServicioId { get; set; }
    public int? MecanicoId { get; set; }
}
