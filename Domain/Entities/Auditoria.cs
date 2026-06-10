namespace Domain.Entities;

/// <summary>
/// Entidad de auditoría automática. EF Core la puebla desde el interceptor en AutoTallerDbContext.SaveChangesAsync.
/// Registra cada INSERT, UPDATE o DELETE con el usuario que lo realizó y los datos afectados.
/// Solo accesible para Admin mediante AuditoriasController.
/// </summary>
public class Auditoria : BaseEntity
{
    /// <summary>Nombre de la tabla/entidad que fue modificada (ej. "Cliente", "Repuesto").</summary>
    public string EntidadAfectada { get; set; } = string.Empty;
    /// <summary>Tipo de operación: "Added" (INSERT), "Modified" (UPDATE), "Deleted" (DELETE).</summary>
    public string Accion { get; set; } = string.Empty;
    /// <summary>ID del usuario que realizó la acción (puede ser null si no hay sesión activa).</summary>
    public int? UsuarioId { get; set; }
    /// <summary>Correo del usuario que realizó la acción, guardado para histórico.</summary>
    public string UsuarioCorreo { get; set; } = string.Empty;
    /// <summary>Fecha y hora UTC en que se realizó la acción.</summary>
    public DateTime FechaAccion { get; set; }
    /// <summary>JSON o texto con los valores antes/después del cambio para trazabilidad.</summary>
    public string Detalles { get; set; } = string.Empty;
}
