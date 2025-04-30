using AutoMapper;
using Car.App.Models.Dto;
using Models.Bridge.Car;

namespace CarService.Profiles;

public class CarProfileForApi : Profile
{
    public CarProfileForApi()
    {
        CreateMap<CarRequestDataDto, AddCarRequest>()
            .ForMember(d => d.Brand,
                opt => opt.MapFrom(s => s.Brand!))
            .ForMember(d => d.Color,
                opt => opt.MapFrom(s => s.Color!))
            .ForMember(d => d.Price,
                opt => opt.MapFrom(s => s.Price))
            
            .ForMember(d => d.UsedCarDetail,
                opt => opt.MapFrom(s => s.UsedCarDetail))
            .ForMember(d => d.ManufacturingDetail,
                opt => opt.MapFrom(s => s.ManufacturingDetail))
            .ForMember(d => d.ManagerDetail,
                opt => opt.MapFrom(s => s.ManagerDetail));
    }
}