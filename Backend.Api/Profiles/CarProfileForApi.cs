using AutoMapper;
using Backend.Api.Models.Requests;
using Backend.Api.Models.Responses;
using Backend.App.Models.Business;
using Backend.App.Models.Commands;

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
                src.PhotoAccessor!.AccessMethod))
            
            .ForMember(dest => dest.Extension, opt => opt.MapFrom(src => src.Data.Extension))
            .ForMember(dest => dest.Id,        opt => opt.MapFrom(src => src.Id));
        
        CreateMap<Car, CarResponse>()
            .ForMember(dest => dest.Photo, opt => opt.MapFrom(src => src.Photo));

        CreateMap<PatchCarRequest, UpdateCarCommand>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PhotoId, opt => opt.Ignore());
    }
}