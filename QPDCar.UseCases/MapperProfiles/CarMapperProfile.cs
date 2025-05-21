using AutoMapper;
using QPDCar.Models.BusinessModels.CarModels;
using QPDCar.UseCases.Models.CarModels;

namespace QPDCar.UseCases.MapperProfiles;

public class CarMapperProfile : Profile
{
    public CarMapperProfile()
    {
        CreateMap<DomainCarsInCart, CarUseCaseResponsePage>()
            .ForMember(dest => dest.Cars,
                opt => opt.Ignore());
    }
}