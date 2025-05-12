using AutoMapper;
using Private.StorageModels;
using Public.Models.BusinessModels.CarModels;
using Public.Models.DtoModels.CarDtoModels;

namespace Private.Services.Profiles;

public class CarProfile : Profile
{
    public CarProfile()
    {
        CreateMap<DtoForCreateCar, CarEntity>()
            .ForMember(dest => dest.Id,               opt => opt.Ignore())
            .ForMember(dest => dest.PrioritySale,     opt => opt.Ignore())
            .ForMember(dest => dest.CarCondition,     opt => opt.Ignore())
            .ForMember(dest => dest.PhotoMetadataId,  opt => opt.Ignore())
            .ForMember(dest => dest.PhotoMetadata,    opt => opt.Ignore())
            .ReverseMap();
        
        CreateMap<CarEntity, DomainCar>()
            .ForMember(d => d.Photo, opt => opt.Ignore());
    }
}