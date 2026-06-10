namespace Api.DTOs;

/// <summary>
/// DTO de salida que representa una Orden de Servicio completa para mostrar al cliente o al frontend.
/// Incluye datos desnormalizados (ServicioNombre, Estado como string) para facilitar la presentación.
/// Estado es la representación textual del enum EstadoOrden: "Pendiente", "Aprobada", "EnReparacion", "Cerrada", "Cancelada".
/// </summary>
public class OrdenServicioDto
{
    /// <summary>ID único de la orden.</summary>
    public int Id { get; set; }
    /// <summary>ID del vehículo al que pertenece esta orden.</summary>
    public int VehiculoId { get; set; }
    /// <summary>ID del servicio del catálogo aplicado (ej. "Cambio de aceite").</summary>
    public int ServicioId { get; set; }
    /// <summary>Nombre del servicio (desnormalizado de CatalogoServicio.Nombre para evitar un Join extra en el frontend).</summary>
    public string ServicioNombre { get; set; } = string.Empty;
    /// <summary>Estado actual de la orden como string: Pendiente | Aprobada | EnReparacion | Cerrada | Cancelada.</summary>
    public string Estado { get; set; } = string.Empty;
    /// <summary>ID del mecánico asignado a esta orden (null si aún no asignado).</summary>
    public int? MecanicoId { get; set; }
    /// <summary>Kilometraje del vehículo al momento de ingreso al taller.</summary>
    public int KilometrajeIngreso { get; set; }
    /// <summary>Fecha y hora de ingreso del vehículo al taller.</summary>
    public DateTime FechaIngreso { get; set; }
    /// <summary>Fecha estimada de entrega al cliente (null si no se ha estimado).</summary>
    public DateTime? FechaEstimadaEntrega { get; set; }
    /// <summary>Indica si el cliente aprobó el presupuesto para iniciar la reparación.</summary>
    public bool AprobadaPorCliente { get; set; }
    
    /// <summary>Lista de repuestos utilizados en esta orden (cada uno con cantidad y precio histórico).</summary>
    public ICollection<DetalleOrdenDto> Detalles { get; set; } = new List<DetalleOrdenDto>();
}
