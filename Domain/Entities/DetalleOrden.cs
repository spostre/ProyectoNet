namespace Domain.Entities;

public class DetalleOrden : BaseEntity
{
    public int OrdenServicioId { get; set; }
    public OrdenServicio? OrdenServicio { get; set; }

    public int RepuestoId { get; set; }
    public Repuesto? Repuesto { get; set; }

    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; } // Precio histórico en el momento de la orden

    public decimal Subtotal => Cantidad * PrecioUnitario;
}
