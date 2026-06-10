using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Entidad que representa un Usuario del sistema (empleado del taller).
/// Los usuarios son los que pueden hacer login y tienen roles asignados.
/// 
/// Roles disponibles (enum RolUsuario): Admin, Mecanico, Recepcionista.
/// El Rol determina qué endpoints puede usar (controlado por [Authorize(Roles="...")] en los controladores).
/// 
/// NOTA: PasswordHash guarda la contraseña sin encriptación actualmente (texto plano, solo para desarrollo/examen).
/// En producción se usaría BCrypt u otro algoritmo de hashing.
/// </summary>
public class Usuario : BaseEntity
{
    /// <summary>Correo electrónico único del usuario, usado como nombre de usuario para el login.</summary>
    public string Correo { get; set; } = string.Empty;
    /// <summary>Contraseña almacenada (actualmente en texto plano; en producción debería ser un hash BCrypt).</summary>
    public string PasswordHash { get; set; } = string.Empty;
    /// <summary>Rol del usuario en el sistema: Admin, Mecanico o Recepcionista.</summary>
    public RolUsuario Rol { get; set; }
}
