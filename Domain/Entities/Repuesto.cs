namespace Domain.Entities;

public class Repuesto : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int CantidadStock { get; private set; }
    public decimal PrecioUnitario { get; set; }

    // Lógica de Dominio: Modificar Stock
    public void AumentarStock(int cantidad)
    {
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a cero.");
        CantidadStock += cantidad;
    }

    public void ReducirStock(int cantidad)
    {
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a cero.");
        if (CantidadStock < cantidad) throw new InvalidOperationException($"No hay suficiente stock del repuesto {Codigo}. Stock actual: {CantidadStock}");
        CantidadStock -= cantidad;
    }
}
