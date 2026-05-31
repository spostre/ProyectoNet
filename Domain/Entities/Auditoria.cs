namespace Domain.Entities;

public class Auditoria : BaseEntity
{
    public string EntidadAfectada { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public int? UsuarioId { get; set; }
    public string UsuarioCorreo { get; set; } = string.Empty;
    public DateTime FechaAccion { get; set; }
    public string Detalles { get; set; } = string.Empty;
}
