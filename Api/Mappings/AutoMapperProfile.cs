using AutoMapper;
using Domain.Entities;
using Api.DTOs;

namespace Api.Mappings;

/// <summary>
/// Perfil de AutoMapper que define las conversiones entre entidades del dominio y DTOs.
/// AutoMapper usa este perfil para transformar objetos automáticamente sin mapeo manual.
/// 
/// Patrón: CreateMap&lt;Origen, Destino&gt;()
///   - ReverseMap(): permite mapear también en sentido inverso (Destino → Origen).
///   - ForMember(): configura cómo mapear una propiedad específica del destino.
/// 
/// Para añadir una nueva entidad: agrega una línea CreateMap&lt;NuevaEntidad, NuevoDto&gt;() aquí.
/// </summary>
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Cliente ↔ ClienteDto (bidireccional)
        CreateMap<Cliente, ClienteDto>().ReverseMap();
        // CreateClienteDto → Cliente (solo hacia la entidad, para crear nuevos clientes)
        CreateMap<CreateClienteDto, Cliente>();

        // Marca ↔ MarcaDto (bidireccional)
        CreateMap<Marca, MarcaDto>().ReverseMap();
        // Modelo → ModeloDto: incluye el nombre de la Marca como campo desnormalizado
        CreateMap<Modelo, ModeloDto>()
            .ForMember(dest => dest.MarcaNombre, opt => opt.MapFrom(src => src.Marca != null ? src.Marca.Nombre : string.Empty))
            .ReverseMap();

        // Proveedor ↔ ProveedorDto (bidireccional)
        CreateMap<Proveedor, ProveedorDto>().ReverseMap();

        // CatalogoServicio ↔ CatalogoServicioDto (bidireccional)
        CreateMap<CatalogoServicio, CatalogoServicioDto>().ReverseMap();

        // CreateVehiculoDto → Vehiculo (solo entrada para crear)
        CreateMap<CreateVehiculoDto, Vehiculo>();
        // Vehiculo → VehiculoDto: añade el nombre de Marca y Modelo para mostrar al cliente
        CreateMap<Vehiculo, VehiculoDto>()
            .ForMember(dest => dest.MarcaNombre, opt => opt.MapFrom(src => src.Modelo != null && src.Modelo.Marca != null ? src.Modelo.Marca.Nombre : string.Empty))
            .ForMember(dest => dest.ModeloNombre, opt => opt.MapFrom(src => src.Modelo != null ? src.Modelo.Nombre : string.Empty))
            .ReverseMap();

        // Repuesto → RepuestoDto: incluye nombre del proveedor
        CreateMap<Repuesto, RepuestoDto>()
            .ForMember(dest => dest.ProveedorNombre, opt => opt.MapFrom(src => src.Proveedor != null ? src.Proveedor.NombreEmpresa : string.Empty))
            .ReverseMap();
        CreateMap<CreateRepuestoDto, Repuesto>();

        // Usuario → UsuarioDto: convierte el enum RolUsuario a string legible
        CreateMap<Usuario, UsuarioDto>()
            .ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.Rol.ToString()))
            .ReverseMap();

        // DetalleOrden → DetalleOrdenDto: incluye descripción del repuesto
        CreateMap<DetalleOrden, DetalleOrdenDto>()
            .ForMember(dest => dest.RepuestoDescripcion, opt => opt.MapFrom(src => src.Repuesto != null ? src.Repuesto.Descripcion : string.Empty))
            .ReverseMap();

        // OrdenServicio → OrdenServicioDto: incluye nombre del servicio y estado como string
        CreateMap<OrdenServicio, OrdenServicioDto>()
            .ForMember(dest => dest.ServicioNombre, opt => opt.MapFrom(src => src.Servicio != null ? src.Servicio.Nombre : string.Empty))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString())) // Enum → string
            .ReverseMap();

        // Factura ↔ FacturaDto y Auditoria ↔ AuditoriaDto (bidireccionales directos)
        CreateMap<Factura, FacturaDto>().ReverseMap();
        CreateMap<Auditoria, AuditoriaDto>().ReverseMap();
    }
}
