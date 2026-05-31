namespace Application.DTOs;

public class DetalleOrdenDto
{
    public int Id { get; set; }
    public int OrdenServicioId { get; set; }
    public int RepuestoId { get; set; }
    public string RepuestoDescripcion { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
