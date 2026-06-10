namespace Api.DTOs;

public class ProveedorDto
{
    public int Id { get; set; }
    public string NombreEmpresa { get; set; } = string.Empty;
    public string Contacto { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
}
