using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class ClienteService : IClienteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ClienteService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ClienteDto> RegistrarClienteConVehiculoAsync(CreateClienteDto dto)
    {
        // 1. Mapear el DTO al objeto de dominio Cliente
        var cliente = _mapper.Map<Cliente>(dto);

        // 2. Persistir el cliente primero para que genere su Id
        await _unitOfWork.Clientes.AddAsync(cliente);
        await _unitOfWork.CommitAsync();

        // 3. Por cada vehículo en el DTO, crear y asociar al cliente recién creado
        foreach (var vehiculoDto in dto.Vehiculos)
        {
            var vehiculo = _mapper.Map<Vehiculo>(vehiculoDto);
            vehiculo.AsignarCliente(cliente.Id);
            await _unitOfWork.Vehiculos.AddAsync(vehiculo);
        }

        // 4. Persistir todos los vehículos en una sola transacción
        if (dto.Vehiculos.Any())
        {
            await _unitOfWork.CommitAsync();
        }

        // 5. Recargar el cliente con sus vehículos incluidos para retornarlo completo
        var result = await _unitOfWork.Clientes.GetByIdAsync(cliente.Id);
        return _mapper.Map<ClienteDto>(result);
    }
}
