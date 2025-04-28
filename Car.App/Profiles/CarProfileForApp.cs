using AutoMapper;
using Car.App.Models.CarModels;
using Car.App.Models.PhotoModels;

namespace Car.App.Profiles;

public class CarProfileForApp : Profile
{
    public CarProfileForApp()
    {
        CreateMap<CarRequest, CarData>();

        CreateMap<PhotoRequest, PhotoData>();
    }
}