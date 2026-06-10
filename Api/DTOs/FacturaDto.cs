namespace Api.DTOs;

public class FacturaDto
{
    public int Id { get; set; }
    public int OrdenServicioId { get; set; }
    public string ResumenServicios { get; set; } = string.Empty;
    public decimal TotalRepuestos { get; set; }
    public decimal TotalManoObra { get; set; }
    public decimal Total { get; set; }
    public DateTime FechaGeneracion { get; set; }
}
