using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class OrdenServicioService : IOrdenServicioService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrdenServicioService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrdenServicioDto> CrearOrdenServicioAsync(CreateOrdenServicioDto dto)
    {
        var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(dto.VehiculoId);
        if (vehiculo == null) throw new Exception("Vehículo no encontrado");

        var tipoServicio = Enum.Parse<TipoServicio>(dto.TipoServicio);

        var nuevaOrden = new OrdenServicio
        {
            VehiculoId = dto.VehiculoId,
            TipoServicio = tipoServicio
        };

        if (dto.MecanicoId.HasValue)
        {
            nuevaOrden.AsignarMecanico(dto.MecanicoId.Value);
        }

        nuevaOrden.CalcularFechaEstimadaEntrega();

        await _unitOfWork.OrdenesServicio.AddAsync(nuevaOrden);
        await _unitOfWork.CommitAsync();

        return _mapper.Map<OrdenServicioDto>(nuevaOrden);
    }

    public async Task<bool> ActualizarOrdenConTrabajoRealizadoAsync(int ordenId, string nuevoEstado, List<DetalleOrdenDto> repuestosUsados)
    {
        var orden = await _unitOfWork.OrdenesServicio.GetByIdAsync(ordenId);
        if (orden == null) return false;

        var estado = Enum.Parse<EstadoOrden>(nuevoEstado);
        orden.CambiarEstado(estado);

        // Procesar repuestos
        foreach (var detalleDto in repuestosUsados)
        {
            var repuesto = await _unitOfWork.Repuestos.GetByIdAsync(detalleDto.RepuestoId);
            if (repuesto != null)
            {
                // Reducir stock
                repuesto.ReducirStock(detalleDto.Cantidad);
                _unitOfWork.Repuestos.Update(repuesto);

                var detalle = new DetalleOrden
                {
                    OrdenServicioId = orden.Id,
                    RepuestoId = repuesto.Id,
                    Cantidad = detalleDto.Cantidad,
                    PrecioUnitario = repuesto.PrecioUnitario
                };
                await _unitOfWork.DetallesOrden.AddAsync(detalle);
            }
        }

        _unitOfWork.OrdenesServicio.Update(orden);
        await _unitOfWork.CommitAsync();

        return true;
    }

    public async Task<FacturaDto> GenerarFacturaAsync(int ordenId)
    {
        var orden = await _unitOfWork.OrdenesServicio.GetByIdAsync(ordenId);
        if (orden == null) throw new Exception("Orden no encontrada");

        // Obtener detalles para calcular total de repuestos
        var detalles = await _unitOfWork.DetallesOrden.FindAsync(d => d.OrdenServicioId == ordenId);
        decimal totalRepuestos = detalles.Sum(d => d.Subtotal);

        // Mano de obra estática por ahora (podría venir de otra tabla)
        decimal totalManoObra = orden.TipoServicio == TipoServicio.Diagnostico ? 50 : 150;

        var factura = new Factura
        {
            OrdenServicioId = ordenId,
            TotalRepuestos = totalRepuestos,
            TotalManoObra = totalManoObra,
            ResumenServicios = $"Servicio de {orden.TipoServicio} para vehículo {orden.VehiculoId}"
        };

        await _unitOfWork.Facturas.AddAsync(factura);
        
        orden.CambiarEstado(EstadoOrden.Cerrada);
        _unitOfWork.OrdenesServicio.Update(orden);

        await _unitOfWork.CommitAsync();

        return _mapper.Map<FacturaDto>(factura);
    }
}
