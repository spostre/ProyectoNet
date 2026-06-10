namespace Api.DTOs;

public class RepuestoDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int CantidadStock { get; set; }
    public decimal PrecioUnitario { get; set; }
    public int? ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = string.Empty;
}
