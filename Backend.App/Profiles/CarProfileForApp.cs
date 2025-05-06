using AutoMapper;
using Backend.App.Models.Business;
using Backend.App.Models.Commands;
using Backend.App.Models.Dto;
using Enum.Common;

namespace Backend.App.Profiles;

public class CarProfileForApp : Profile
{
    public CarProfileForApp()
    {
        CreateMap<CarDto, Car>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id ?? 0))
            .ForMember(d => d.Brand,
                o => o.MapFrom(src => src.Brand ?? string.Empty))
            .ForMember(d => d.Color,
                o => o.MapFrom(src => src.Color ?? string.Empty))
            .ForMember(d => d.Price,
                o => o.MapFrom(src => src.Price ?? -1))
            .ForMember(d => d.PrioritySale,
                opt => opt.MapFrom(src => src.PrioritySale.ToString()))
            .ForMember(d => d.Condition,
                o => o.MapFrom(src => src.Condition.ToString()))
            .ForMember(d => d.CurrentOwner, o => o.MapFrom(src => src.CurrentOwner!.ToString()))
            .ForMember(d => d.Mileage, o => o.MapFrom(src => src.Mileage!))
            .ForMember(d => d.Photo, o => o.Ignore());
        
        CreateMap<PhotoDto, Photo>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(src => src.Id ?? Guid.Empty))
            .ForMember(dest => dest.Storage,
                opt => opt.MapFrom(src => src.StorageType ?? PhotoStorageType.NotExists))
            .ForMember(dest => dest.Data,
                opt => opt.MapFrom(src => new PhotoData 
                    {
                    Data = src.Data ?? Array.Empty<byte>(),
                    Extension = src.Extension ?? PhotoFileExtension.EmptyOrUnknown,
                }))
            .ForMember(dest => dest.PhotoAccessor,
                opt => opt.Ignore());

        CreateMap<UpdateCarCommand, CarDto>();

        CreateMap<SearchCarByQueryCommand, CarQueryDto>();
    }
}