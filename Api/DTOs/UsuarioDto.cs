namespace Api.DTOs;

public class UsuarioDto
{
    public int Id { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}
