namespace Application.DTOs;

public class CreateUsuarioDto
{
    public string Correo { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    
    // Puede ser "Admin", "Mecanico" o "Recepcionista"
    public string Rol { get; set; } = string.Empty;
}
