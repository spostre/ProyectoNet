namespace Domain.Entities;

public class Factura : BaseEntity
{
    public int OrdenServicioId { get; set; }
    public OrdenServicio? OrdenServicio { get; set; }

    public string ResumenServicios { get; set; } = string.Empty;
    public decimal TotalRepuestos { get; set; }
    public decimal TotalManoObra { get; set; }
    
    public decimal Total => TotalRepuestos + TotalManoObra;

    public DateTime FechaGeneracion { get; set; }

    public Factura()
    {
        FechaGeneracion = DateTime.UtcNow;
    }
}
