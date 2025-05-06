using AutoMapper;
using Backend.Api.Models.Requests;
using Backend.Api.Models.Responses;
using Backend.App.Models.Business;
using Backend.App.Models.Commands;
using Enum.Common;

namespace Backend.Api.Profiles;

public class CarProfileForApi : Profile
{
    public CarProfileForApi()
    {
        CreateMap<AddCarRequest, CreateCarCommand>();

        CreateMap<Photo, PhotoResponse>()
            .ForMember(dest => dest.Access, opt => opt.MapFrom(src =>
                src.PhotoAccessor!.Access()))
            .ForMember(dest => dest.AccessMethod, opt => opt.MapFrom(src =>
                src.PhotoAccessor!.AccessMethod.ToString()))
            .ForMember(dest => dest.Extension, opt => opt.MapFrom(src => src.Data.Extension.ToString()))
            .ForMember(dest => dest.Id,        opt => opt.MapFrom(src => src.Id));
        
        CreateMap<Car, CarResponse>()
            .ForMember(dest => dest.Photo, opt => opt.MapFrom(src => src.Photo))
            .ForMember(dest => dest.Condition, opt => opt.MapFrom(src => src.Condition.ToString()))
            .ForMember(dest => dest.PrioritySale, opt => opt.MapFrom(src => src.PrioritySale.ToString()));

        CreateMap<PatchCarRequest, UpdateCarCommand>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PhotoId, opt => opt.Ignore());
        
        CreateMap<CarQueryRequest, SearchCarByQueryCommand>()
            .ForMember(dest => dest.PhotoTerm,
                opt => opt.MapFrom(src => src.PhotoTerm ?? PhotoHavingTerm.NoMatter))
            .ForMember(dest => dest.Direction,
                opt => opt.MapFrom(src => src.Direction ?? SortDirection.Ascending))
            .ForMember(dest => dest.PageNumber,
                opt => opt.MapFrom(src => src.PageNumber ?? 1))
            .ForMember(dest => dest.PageSize,
                opt => opt.MapFrom(src => src.PageSize ?? 10));
    }
}