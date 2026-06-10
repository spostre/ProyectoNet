namespace Domain.Entities;

/// <summary>
/// Tabla intermedia entre OrdenServicio y Repuesto. Representa los repuestos usados en una orden.
/// Cada DetalleOrden registra qué repuesto se usó, cuántas unidades, y el precio en ese momento.
/// 
/// IMPORTANTE: PrecioUnitario guarda el precio histórico al momento de la orden.
/// Aunque el precio del repuesto cambie después, la factura refleja el precio original.
/// El Subtotal es una propiedad calculada: Cantidad × PrecioUnitario (no se guarda en BD).
/// </summary>
public class DetalleOrden : BaseEntity
{
    /// <summary>FK a la OrdenServicio a la que pertenece este detalle.</summary>
    public int OrdenServicioId { get; set; }
    public OrdenServicio? OrdenServicio { get; set; }

    /// <summary>FK al Repuesto utilizado en este detalle.</summary>
    public int RepuestoId { get; set; }
    public Repuesto? Repuesto { get; set; }

    /// <summary>Número de unidades del repuesto utilizadas.</summary>
    public int Cantidad { get; set; }
    /// <summary>Precio unitario del repuesto al momento de crear la orden (precio histórico, no el actual).</summary>
    public decimal PrecioUnitario { get; set; } // Precio histórico en el momento de la orden

    /// <summary>Subtotal calculado: Cantidad × PrecioUnitario. No se almacena en BD.</summary>
    public decimal Subtotal => Cantidad * PrecioUnitario;
}
