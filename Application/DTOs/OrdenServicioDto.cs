namespace Application.DTOs;

public class OrdenServicioDto
{
    public int Id { get; set; }
    public int VehiculoId { get; set; }
    public string TipoServicio { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int? MecanicoId { get; set; }
    public DateTime FechaIngreso { get; set; }
    public DateTime? FechaEstimadaEntrega { get; set; }
    public bool AprobadaPorCliente { get; set; }
    
    public ICollection<DetalleOrdenDto> Detalles { get; set; } = new List<DetalleOrdenDto>();
}
