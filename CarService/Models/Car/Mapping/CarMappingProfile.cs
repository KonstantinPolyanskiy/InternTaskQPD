using AutoMapper;
using CarService.Models.Car.Requests;

namespace CarService.Models.Car.Mapping;

public class CarMappingProfile : Profile
{
    public CarMappingProfile()
    {
        CreateMap<CreateCarRequestDto, BaseCar>();
        CreateMap<CreateCarRequestDto, SecondHandCar>();
        
        CreateMap<PatchUpdateCarRequestDto, BaseCar>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        
        CreateMap<PatchUpdateCarRequestDto, SecondHandCar>()
            .IncludeBase<PatchUpdateCarRequestDto, BaseCar>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}