using Application.DTOs;

namespace Application.Interfaces;

public interface IOrdenServicioService
{
    Task<OrdenServicioDto> CrearOrdenServicioAsync(CreateOrdenServicioDto dto);
    Task<bool> ActualizarOrdenConTrabajoRealizadoAsync(int ordenId, string nuevoEstado, List<DetalleOrdenDto> repuestosUsados);
    Task<FacturaDto> GenerarFacturaAsync(int ordenId);
}
