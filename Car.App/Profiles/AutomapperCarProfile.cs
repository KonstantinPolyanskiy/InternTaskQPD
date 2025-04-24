using AutoMapper;
using Car.App.Models;
using Contracts.Dtos;
using Contracts.Shared;

namespace Car.App.Profiles;

public class AutomapperCarProfile : Profile
{
    public AutomapperCarProfile()
    {
        // DTO Add Car Domain -> DTO Save Car DAL
        CreateMap<AddedCarServicesDto, AddedCarDataLayerDto>()
            .ForMember(d => d.CarType,
                o => o.MapFrom((src, _, _, ctx)
                    => (CarTypes)ctx.Items[nameof(CarTypes)]));
        
        // DTO Patch Car Domain -> DTO Update Car DAL
        CreateMap<PatchCarServicesDto, UpdatedCarDataLayerDto>();
        
        // Car DAL -> Car Domain
        CreateMap<Dal.Models.Car, UsedCar>();
        CreateMap<Dal.Models.Car, NewCar>();
        
        // DTO Updated Car DAL -> Entity Car DAL
        CreateMap<UpdatedCarDataLayerDto, Car.Dal.Models.Car>()
            .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));
        
        CreateMap<ApplicationPhotoModel, Car.Dal.Models.Photo>()
            .ForAllMembers(o => o.Condition((s, d, sm) => sm != null));
    }

}