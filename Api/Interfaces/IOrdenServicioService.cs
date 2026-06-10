using Api.DTOs;

namespace Api.Interfaces;

public interface IOrdenServicioService
{
    Task<OrdenServicioDto> CrearOrdenServicioAsync(CreateOrdenServicioDto dto);
    Task<bool> ActualizarOrdenConTrabajoRealizadoAsync(int ordenId, string nuevoEstado, List<DetalleOrdenDto> repuestosUsados);
    Task<bool> AprobarOrdenAsync(int ordenId);
    Task<FacturaDto> GenerarFacturaAsync(int ordenId);
}
