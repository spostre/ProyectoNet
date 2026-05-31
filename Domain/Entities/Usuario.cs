using Domain.Enums;

namespace Domain.Entities;

public class Usuario : BaseEntity
{
    public string Correo { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
}
