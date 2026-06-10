namespace Domain.Entities;

/// <summary>
/// Entidad que representa un Proveedor de repuestos.
/// Un Proveedor puede suministrar múltiples repuestos (relación 1:N con Repuesto).
/// Los datos del proveedor sirven para contacto y rastreo de origen del inventario.
/// </summary>
public class Proveedor : BaseEntity
{
    /// <summary>Nombre de la empresa proveedora (ej. "AutoPartes del Norte S.A.").</summary>
    public string NombreEmpresa { get; set; } = string.Empty;
    /// <summary>Nombre de la persona de contacto dentro de la empresa proveedora.</summary>
    public string Contacto { get; set; } = string.Empty;
    /// <summary>Teléfono de contacto del proveedor.</summary>
    public string Telefono { get; set; } = string.Empty;
    /// <summary>Correo electrónico del proveedor para pedidos.</summary>
    public string Correo { get; set; } = string.Empty;

    /// <summary>Repuestos suministrados por este proveedor (relación 1:N).</summary>
    public ICollection<Repuesto> Repuestos { get; set; } = new List<Repuesto>();
}
