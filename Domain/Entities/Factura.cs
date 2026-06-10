namespace Domain.Entities;

/// <summary>
/// Entidad que representa la Factura generada al cerrar una Orden de Servicio.
/// Se crea a través de OrdenServicioService.GenerarFacturaAsync() cuando la orden se cierra.
/// 
/// El Total es una propiedad calculada (no almacenada en BD): TotalRepuestos + TotalManoObra.
/// La FechaGeneracion se asigna automáticamente en el constructor a la fecha actual UTC.
/// </summary>
public class Factura : BaseEntity
{
    /// <summary>FK a la OrdenServicio que origina esta factura (relación 1:1).</summary>
    public int OrdenServicioId { get; set; }
    public OrdenServicio? OrdenServicio { get; set; }

    /// <summary>Texto resumen de los servicios realizados (ej. "Cambio de aceite, Revisión de frenos").</summary>
    public string ResumenServicios { get; set; } = string.Empty;
    /// <summary>Suma del costo de todos los repuestos usados en la orden.</summary>
    public decimal TotalRepuestos { get; set; }
    /// <summary>Suma del costo de mano de obra de todos los servicios realizados.</summary>
    public decimal TotalManoObra { get; set; }
    
    /// <summary>Total calculado automáticamente: TotalRepuestos + TotalManoObra. No se almacena en BD.</summary>
    public decimal Total => TotalRepuestos + TotalManoObra;

    /// <summary>Fecha y hora UTC en que se emitió la factura.</summary>
    public DateTime FechaGeneracion { get; set; }

    /// <summary>Constructor que inicializa la FechaGeneracion con la hora actual UTC.</summary>
    public Factura()
    {
        FechaGeneracion = DateTime.UtcNow;
    }
}
