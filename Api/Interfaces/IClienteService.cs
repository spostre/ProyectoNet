using Api.DTOs;

namespace Api.Interfaces;

public interface IClienteService
{
    /// <summary>
    /// Registra un nuevo cliente y, de forma simultánea, asocia uno o más vehículos a su cuenta.
    /// </summary>
    Task<ClienteDto> RegistrarClienteConVehiculoAsync(CreateClienteDto dto);
}
