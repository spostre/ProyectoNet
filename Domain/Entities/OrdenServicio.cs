using Domain.Enums;

namespace Domain.Entities;

public class OrdenServicio : BaseEntity
{
    public int VehiculoId { get; set; }
    public Vehiculo? Vehiculo { get; set; }

    public TipoServicio TipoServicio { get; set; }
    public EstadoOrden Estado { get; private set; }

    public int? MecanicoId { get; private set; }
    public Usuario? Mecanico { get; set; }

    public DateTime FechaIngreso { get; set; }
    public DateTime? FechaEstimadaEntrega { get; private set; }

    public bool AprobadaPorCliente { get; private set; }

    public ICollection<DetalleOrden> Detalles { get; set; } = new List<DetalleOrden>();

    public OrdenServicio()
    {
        Estado = EstadoOrden.Ingresada;
        FechaIngreso = DateTime.UtcNow;
        AprobadaPorCliente = false;
    }

    // Lógica de Dominio
    public void AsignarMecanico(int mecanicoId)
    {
        MecanicoId = mecanicoId;
    }

    public void AprobarOrden()
    {
        AprobadaPorCliente = true;
        Estado = EstadoOrden.EnReparacion;
    }

    public void CambiarEstado(EstadoOrden nuevoEstado)
    {
        // Reglas simples de transición podrían ir aquí
        Estado = nuevoEstado;
    }

    // Método a ajustar según las reglas futuras
    public void CalcularFechaEstimadaEntrega()
    {
        FechaEstimadaEntrega = TipoServicio switch
        {
            TipoServicio.Diagnostico => FechaIngreso.AddDays(1),
            TipoServicio.MantenimientoPreventivo => FechaIngreso.AddDays(2),
            TipoServicio.Reparacion => FechaIngreso.AddDays(5),
            _ => FechaIngreso.AddDays(1)
        };
    }
}
