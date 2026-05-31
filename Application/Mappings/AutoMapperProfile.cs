using AutoMapper;
using Domain.Entities;
using Application.DTOs;

namespace Application.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Cliente, ClienteDto>().ReverseMap();
        CreateMap<CreateClienteDto, Cliente>();
        CreateMap<CreateVehiculoDto, Vehiculo>();
        CreateMap<Vehiculo, VehiculoDto>().ReverseMap();
        CreateMap<Repuesto, RepuestoDto>().ReverseMap();
        CreateMap<CreateRepuestoDto, Repuesto>();
        CreateMap<Usuario, UsuarioDto>()
            .ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.Rol.ToString()))
            .ReverseMap();

        CreateMap<DetalleOrden, DetalleOrdenDto>()
            .ForMember(dest => dest.RepuestoDescripcion, opt => opt.MapFrom(src => src.Repuesto != null ? src.Repuesto.Descripcion : string.Empty))
            .ReverseMap();

        CreateMap<OrdenServicio, OrdenServicioDto>()
            .ForMember(dest => dest.TipoServicio, opt => opt.MapFrom(src => src.TipoServicio.ToString()))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()))
            .ReverseMap();

        CreateMap<Factura, FacturaDto>().ReverseMap();
        CreateMap<Auditoria, AuditoriaDto>().ReverseMap();
    }
}
