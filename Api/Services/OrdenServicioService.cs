using Api.DTOs;
using Api.Interfaces;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Api.Services;

public class OrdenServicioService : IOrdenServicioService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrdenServicioService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Crea una nueva orden de servicio para un vehículo e inicializa los parámetros por defecto.
    /// </summary>
    public async Task<OrdenServicioDto> CrearOrdenServicioAsync(CreateOrdenServicioDto dto)
    {
        // 1. Validar que el vehículo exista
        var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(dto.VehiculoId);
        if (vehiculo == null) throw new Exception("Vehículo no encontrado");

        // 2. Validar que el servicio solicitado esté en el catálogo de tarifas
        var servicio = await _unitOfWork.CatalogoServicios.GetByIdAsync(dto.ServicioId);
        if (servicio == null) throw new Exception("Servicio no encontrado en el catálogo");

        // 3. Crear entidad asignando el kilometraje del vehículo como kilometraje de ingreso
        var nuevaOrden = new OrdenServicio
        {
            VehiculoId = dto.VehiculoId,
            ServicioId = dto.ServicioId,
            KilometrajeIngreso = vehiculo.Kilometraje
        };

        // 4. Si se especifica un mecánico, asignarlo mediante el método de dominio
        if (dto.MecanicoId.HasValue)
        {
            nuevaOrden.AsignarMecanico(dto.MecanicoId.Value);
        }

        // 5. Calcular la fecha tentativa de finalización
        nuevaOrden.CalcularFechaEstimadaEntrega();

        // 6. Guardar en BD de forma persistente
        await _unitOfWork.OrdenesServicio.AddAsync(nuevaOrden);
        await _unitOfWork.CommitAsync();

        return _mapper.Map<OrdenServicioDto>(nuevaOrden);
    }

    /// <summary>
    /// Actualiza el estado de la orden (ej: "ListaParaEntrega") y registra los repuestos físicos utilizados reduciendo su stock del almacén.
    /// </summary>
    public async Task<bool> ActualizarOrdenConTrabajoRealizadoAsync(int ordenId, string nuevoEstado, List<DetalleOrdenDto> repuestosUsados)
    {
        // 1. Buscar la orden de servicio en BD
        var orden = await _unitOfWork.OrdenesServicio.GetByIdAsync(ordenId);
        if (orden == null) return false;

        // 2. Parsear y actualizar el estado
        var estado = Enum.Parse<EstadoOrden>(nuevoEstado);
        orden.CambiarEstado(estado);

        // 3. Procesar repuestos e insumos reportados por el mecánico
        foreach (var detalleDto in repuestosUsados)
        {
            var repuesto = await _unitOfWork.Repuestos.GetByIdAsync(detalleDto.RepuestoId);
            if (repuesto != null)
            {
                // Reducir stock del almacén físico (lanza excepción si no hay suficiente stock)
                repuesto.ReducirStock(detalleDto.Cantidad);
                _unitOfWork.Repuestos.Update(repuesto);

                // Crear registro de detalle para asociar el repuesto y precio unitario a la orden
                var detalle = new DetalleOrden
                {
                    OrdenServicioId = orden.Id,
                    RepuestoId = repuesto.Id,
                    Cantidad = detalleDto.Cantidad,
                    PrecioUnitario = repuesto.PrecioUnitario // Se congela el precio histórico
                };
                await _unitOfWork.DetallesOrden.AddAsync(detalle);
            }
        }

        // 4. Guardar todos los cambios atómicamente
        _unitOfWork.OrdenesServicio.Update(orden);
        await _unitOfWork.CommitAsync();

        return true;
    }

    /// <summary>
    /// Registra la aprobación del presupuesto por parte del cliente y cambia el estado a EnReparacion.
    /// </summary>
    public async Task<bool> AprobarOrdenAsync(int ordenId)
    {
        // 1. Buscar orden
        var orden = await _unitOfWork.OrdenesServicio.GetByIdAsync(ordenId);
        if (orden == null) return false;

        // 2. Ejecutar lógica de dominio para aprobación
        orden.AprobarOrden();
        
        // 3. Guardar cambios en BD
        _unitOfWork.OrdenesServicio.Update(orden);
        await _unitOfWork.CommitAsync();

        return true;
    }

    /// <summary>
    /// Genera la liquidación financiera sumando repuestos usados y mano de obra, y cierra permanentemente la orden.
    /// </summary>
    public async Task<FacturaDto> GenerarFacturaAsync(int ordenId)
    {
        // 1. Validar orden existente
        var orden = await _unitOfWork.OrdenesServicio.GetByIdAsync(ordenId);
        if (orden == null) throw new Exception("Orden no encontrada");

        // 2. Cargar tarifa de mano de obra del catálogo asociado
        var servicio = await _unitOfWork.CatalogoServicios.GetByIdAsync(orden.ServicioId);
        if (servicio == null) throw new Exception("Catálogo de servicio no encontrado");

        // 3. Sumar el costo acumulado de repuestos
        var detalles = await _unitOfWork.DetallesOrden.FindAsync(d => d.OrdenServicioId == ordenId);
        decimal totalRepuestos = detalles.Sum(d => d.Subtotal);

        // 4. Establecer mano de obra
        decimal totalManoObra = servicio.PrecioManoObra;

        // 5. Instanciar factura relacionándola a la orden
        var factura = new Factura
        {
            OrdenServicioId = ordenId,
            TotalRepuestos = totalRepuestos,
            TotalManoObra = totalManoObra,
            ResumenServicios = $"Servicio de {servicio.Nombre} para vehículo ID {orden.VehiculoId}"
        };

        await _unitOfWork.Facturas.AddAsync(factura);
        
        // 6. Cerrar el estado de la orden para evitar modificaciones o repuestos extras
        orden.CambiarEstado(EstadoOrden.Cerrada);
        _unitOfWork.OrdenesServicio.Update(orden);

        // 7. Persistir todo en una sola transacción
        await _unitOfWork.CommitAsync();

        return _mapper.Map<FacturaDto>(factura);
    }
}
