using AutoMapper;
using Car.App.Models.CarModels;
using Car.App.Models.Dto;
using Car.App.Models.PhotoModels;

namespace Car.App.Profiles;

public class CarProfileForApp : Profile
{
    public CarProfileForApp()
    {
        CreateMap<CarRequestDataDto, CarDto>();

        CreateMap<PhotoRequest, PhotoData>();
    }
}