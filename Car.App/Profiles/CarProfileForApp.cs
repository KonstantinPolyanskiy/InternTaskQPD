using AutoMapper;
using Car.App.Models;
using Contracts.Dtos;
using Contracts.Shared;
using Contracts.Types;

namespace Car.App.Profiles;

public class CarProfileForApp : Profile
{
    public CarProfileForApp()
    {
        #region Update Entity <-> Domain

        CreateMap<PatchCarDomain, PatchCarEntity>()
            .ForMember(d => d.Id,
                o => o.MapFrom((src, dest, _, ctx) => (int)ctx.Items["Id"]))
            .ForAllMembers(o =>
                o.Condition((src, dest, srcMember) => srcMember != null));
        
        CreateMap<PatchCarEntity, Dal.Models.Car>()
            .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));

        #endregion
        
        #region Add Entity <-> Domain
        
        CreateMap<AddCarDomain, AddCarEntity>()
            .ForMember(d => d.CarType,
                o => o.MapFrom((src, _, _, ctx)
                    => (CarTypes)ctx.Items["CarType"]));
        
        
        CreateMap<Dal.Models.Car, DomainCar>()
            .ForMember(dst => dst.CarType, opt => opt.MapFrom(src => src.CarType));
        
        #endregion
        
        #region Photo Model <-> Domain
        
        CreateMap<CarPhoto, ApplicationPhotoModel>()
            .ForAllMembers(o => 
                o.Condition((src, dest, srcMember) => srcMember != null));
                
        CreateMap<ApplicationPhotoModel, Car.Dal.Models.Photo>()
            .ForAllMembers(o => o.Condition((s, d, sm) => sm != null));
        
        #endregion
    }
}