using Domain.Enums;

namespace Domain.Entities;

public class OrdenServicio : BaseEntity
{
    /// <summary>
    /// Identificador del vehículo asociado (Llave Foránea).
    /// </summary>
    public int VehiculoId { get; set; }
    
    /// <summary>
    /// Propiedad navegacional del Vehículo.
    /// </summary>
    public Vehiculo? Vehiculo { get; set; }

    /// <summary>
    /// Identificador del Servicio a realizar del Catálogo (Mano de obra y descripción).
    /// </summary>
    public int ServicioId { get; set; }
    
    /// <summary>
    /// Propiedad navegacional del Servicio del catálogo.
    /// </summary>
    public CatalogoServicio? Servicio { get; set; }
    
    /// <summary>
    /// Kilometraje registrado en el odómetro del auto al ingresar al taller.
    /// </summary>
    public int KilometrajeIngreso { get; set; }
    
    /// <summary>
    /// Estado actual del flujo de reparación (Ingresada, Aprobada, EnReparacion, ListaParaEntrega, Cerrada, Cancelada).
    /// </summary>
    public EstadoOrden Estado { get; private set; }

    /// <summary>
    /// Identificador del Mecánico (Usuario con rol Mecánico) asignado.
    /// </summary>
    public int? MecanicoId { get; private set; }
    
    /// <summary>
    /// Propiedad navegacional del Mecánico asignado.
    /// </summary>
    public Usuario? Mecanico { get; set; }

    /// <summary>
    /// Fecha y hora de ingreso del vehículo al taller.
    /// </summary>
    public DateTime FechaIngreso { get; set; }
    
    /// <summary>
    /// Fecha tentativa de finalización y entrega al cliente.
    /// </summary>
    public DateTime? FechaEstimadaEntrega { get; private set; }

    /// <summary>
    /// Indica si el cliente revisó y autorizó el presupuesto inicial de mano de obra y orden.
    /// </summary>
    public bool AprobadaPorCliente { get; private set; }

    /// <summary>
    /// Desglose de repuestos e insumos físicos utilizados durante el servicio.
    /// </summary>
    public ICollection<DetalleOrden> Detalles { get; set; } = new List<DetalleOrden>();

    public OrdenServicio()
    {
        // Estado por defecto al crear una orden
        Estado = EstadoOrden.Ingresada;
        FechaIngreso = DateTime.UtcNow;
        AprobadaPorCliente = false;
    }

    /// <summary>
    /// Lógica de Negocio: Asigna un mecánico de forma explícita a la orden de taller.
    /// </summary>
    /// <param name="mecanicoId">Id del usuario mecánico asignado.</param>
    public void AsignarMecanico(int mecanicoId)
    {
        if (Estado == EstadoOrden.Cerrada || Estado == EstadoOrden.Cancelada)
            throw new InvalidOperationException("No se puede asignar un mecánico a una orden cerrada o cancelada.");
        MecanicoId = mecanicoId;
    }

    /// <summary>
    /// Lógica de Negocio: Registra la aprobación del cliente para pasar la orden a etapa de trabajo.
    /// </summary>
    public void AprobarOrden()
    {
        if (Estado != EstadoOrden.Ingresada)
            throw new InvalidOperationException("Solo se pueden aprobar órdenes que estén en estado Ingresada.");
        
        AprobadaPorCliente = true;
        Estado = EstadoOrden.EnReparacion; // Pasa automáticamente a reparación tras aprobación
    }

    /// <summary>
    /// Lógica de Negocio: Transiciona el estado de la orden respetando reglas del flujo de trabajo.
    /// </summary>
    public void CambiarEstado(EstadoOrden nuevoEstado)
    {
        if (Estado == EstadoOrden.Cerrada)
            throw new InvalidOperationException("No se pueden hacer modificaciones a una orden que ya está Cerrada.");

        if (nuevoEstado == EstadoOrden.EnReparacion && !AprobadaPorCliente)
            throw new InvalidOperationException("No se puede iniciar la reparación si el cliente no ha aprobado la orden primero.");

        Estado = nuevoEstado;
    }

    /// <summary>
    /// Lógica de Negocio: Calcula y setea la fecha de entrega sugerida (2 días a partir de la fecha de ingreso).
    /// </summary>
    public void CalcularFechaEstimadaEntrega()
    {
        FechaEstimadaEntrega = FechaIngreso.AddDays(2);
    }
}
