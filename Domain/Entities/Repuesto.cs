namespace Domain.Entities;

/// <summary>
/// Entidad que representa un Repuesto del inventario del taller.
/// Encapsula la lógica de stock mediante métodos de dominio (AumentarStock, ReducirStock).
/// CantidadStock es de solo escritura pública: solo se puede modificar mediante los métodos.
/// Pertenece a un Proveedor (FK opcional).
/// </summary>
public class Repuesto : BaseEntity
{
    /// <summary>Código único del repuesto (ej. "ACE-5W30-1L"). Sirve para identificarlo rápidamente.</summary>
    public string Codigo { get; set; } = string.Empty;
    /// <summary>Descripción legible del repuesto (ej. "Aceite de motor 5W-30 1 litro").</summary>
    public string Descripcion { get; set; } = string.Empty;
    /// <summary>
    /// Cantidad de unidades disponibles en inventario.
    /// Solo se puede modificar a través de AumentarStock() o ReducirStock() para garantizar validaciones.
    /// </summary>
    public int CantidadStock { get; private set; }
    /// <summary>Precio de venta por unidad (en la moneda del sistema).</summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>FK al Proveedor que suministra este repuesto (opcional).</summary>
    public int? ProveedorId { get; set; }
    public Proveedor? Proveedor { get; set; }

    // Lógica de Dominio: Modificar Stock
    /// <summary>
    /// Aumenta el stock del repuesto en la cantidad indicada.
    /// Lanza ArgumentException si la cantidad no es positiva.
    /// </summary>
    public void AumentarStock(int cantidad)
    {
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a cero.");
        CantidadStock += cantidad;
    }

    /// <summary>
    /// Reduce el stock del repuesto en la cantidad indicada.
    /// Lanza ArgumentException si la cantidad es inválida.
    /// Lanza InvalidOperationException si no hay suficiente stock (evita stock negativo).
    /// </summary>
    public void ReducirStock(int cantidad)
    {
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a cero.");
        if (CantidadStock < cantidad) throw new InvalidOperationException($"No hay suficiente stock del repuesto {Codigo}. Stock actual: {CantidadStock}");
        CantidadStock -= cantidad;
    }
}
