using AutoMapper;
using Car.App.Models;
using CarService.Converters;
using CarService.Models;
using Contracts.Dtos;
using Contracts.Shared;

namespace CarService.Profiles;

public class CarProfileForApi : Profile
{
    public CarProfileForApi()
    {
        CreateMap<IFormFile, ApplicationPhotoModel>()
            .ConvertUsing<FormFileToApplicationPhotoConverter>();
        
         // creation request -> DTO Add Car Domain
         CreateMap<AddCarRequest, AddCarDomain>()
             .ForMember(dst => dst.Photo,
                 opt => opt.MapFrom(src => src.Photo));
         
         // patch request -> DTO Update Car Domain 
         CreateMap<PatchCarRequest, PatchCarDomain>();

         CreateMap<DomainCar, CarResponse>();
    }
}