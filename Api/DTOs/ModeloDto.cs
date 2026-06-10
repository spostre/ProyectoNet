namespace Api.DTOs;

public class ModeloDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int MarcaId { get; set; }
    public string MarcaNombre { get; set; } = string.Empty;
}
