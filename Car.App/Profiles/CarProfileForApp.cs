using AutoMapper;
using Car.App.Converters;
using Car.App.Models;
using Contracts.Dtos;
using Contracts.Shared;
using Contracts.Types;

namespace Car.App.Profiles;

public class CarProfileForApp : Profile
{
    public CarProfileForApp()
    {
        // DTO Add Car Domain -> DTO Save Car DAL
        CreateMap<AddedCarServicesDto, AddedCarDataLayerDto>()
            .ForMember(d => d.CarType,
                o => o.MapFrom((src, _, _, ctx)
                    => (CarTypes)ctx.Items["CarType"]));
        
        // DTO Patch Car Domain -> DTO Update Car DAL
        CreateMap<PatchCarServicesDto, UpdatedCarDataLayerDto>();
        
        // Car DAL -> Car Domain
        CreateMap<Dal.Models.Car, ICar>()
            .ConvertUsing<EntityCarToDomainCarConverter>();
        
        // DTO Updated Car DAL -> Entity Car DAL
        CreateMap<UpdatedCarDataLayerDto, Car.Dal.Models.Car>()
            .ForMember(dst => dst.CarType,
                opt => opt.MapFrom(src => (byte)src.CarType))
            .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));
        
                
        // Application Photo -> Entity Photo DAL
        CreateMap<ApplicationPhotoModel, Car.Dal.Models.Photo>()
            .ForAllMembers(o => o.Condition((s, d, sm) => sm != null));

        // Entity Photo DAL -> Domain Photo Model
        CreateMap<Car.Dal.Models.Photo, ApplicationPhotoModel>()
            .ForMember(dest => dest.Content,
                opt => opt.MapFrom(src =>
                    src.PhotoBytes != null ? new MemoryStream(src.PhotoBytes) : null))
            .ForMember(dest => dest.FileExtension,
                opt => opt.MapFrom(src => src.Extension))
            .ForMember(dest => dest.FileName,
                opt => opt.MapFrom(src => src.FileName))
            .ForMember(dest => dest.Length,
                opt => opt.MapFrom(src => src.PhotoBytes!.LongLength));
    }

}