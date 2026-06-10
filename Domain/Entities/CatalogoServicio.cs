namespace Domain.Entities;

/// <summary>
/// Entidad que representa un tipo de servicio disponible en el taller (ej. "Cambio de aceite", "Alineación").
/// Al crear una OrdenServicio, se le asocia un CatalogoServicio para definir qué tipo de trabajo se realiza.
/// PrecioManoObra es el costo del servicio SIN contar los repuestos (éstos van en DetalleOrden).
/// El total de la factura = suma de PrecioManoObra de servicios + suma de Subtotales de DetalleOrden.
/// </summary>
public class CatalogoServicio : BaseEntity
{
    /// <summary>Nombre corto del servicio (ej. "Cambio de aceite").</summary>
    public string Nombre { get; set; } = string.Empty;
    /// <summary>Descripción detallada del servicio (ej. pasos o alcance del trabajo).</summary>
    public string Descripcion { get; set; } = string.Empty;
    /// <summary>Costo de mano de obra del servicio (sin repuestos).</summary>
    public decimal PrecioManoObra { get; set; }

    /// <summary>Órdenes de servicio que usan este tipo de servicio (relación N:M a través de OrdenServicio).</summary>
    public ICollection<OrdenServicio> OrdenesServicio { get; set; } = new List<OrdenServicio>();
}
