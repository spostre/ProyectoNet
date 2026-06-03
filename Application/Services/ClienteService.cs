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

        // 2. Registrar el cliente en el contexto (sin commit aún)
        await _unitOfWork.Clientes.AddAsync(cliente);

        // 3. Por cada vehículo en el DTO, crear y agregar al contexto asociado al cliente
        foreach (var vehiculoDto in dto.Vehiculos)
        {
            // Comprobar si ya existe un vehículo con el mismo VIN
            var existentes = await _unitOfWork.Vehiculos.FindAsync(v => v.Vin == vehiculoDto.Vin);
            var existente = existentes.FirstOrDefault();
            if (existente != null)
            {
                throw new InvalidOperationException($"Ya existe un vehículo con VIN '{vehiculoDto.Vin}'.");
            }

            var vehiculo = _mapper.Map<Vehiculo>(vehiculoDto);
            // Asignamos directamente la entidad navegacional para que EF resuelva el FK en memoria
            vehiculo.Cliente = cliente;
            await _unitOfWork.Vehiculos.AddAsync(vehiculo);
        }

        // 4. Persistir cliente y todos sus vehículos en una única transacción atómica
        await _unitOfWork.CommitAsync();

        // 5. Recargar el cliente con sus vehículos incluidos para retornarlo completo
        var result = await _unitOfWork.Clientes.GetByIdAsync(cliente.Id);
        return _mapper.Map<ClienteDto>(result);
    }
}
