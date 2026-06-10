namespace Api.DTOs;

public class CatalogoServicioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal PrecioManoObra { get; set; }
}
