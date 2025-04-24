using AutoMapper;
using Car.App.Models;
using CarService.Converters;
using Contracts.Shared;
using CarService.Models;
using Contracts.Dtos;

namespace CarService.Profiles;

public class AutomapperCarProfile : Profile
{
    public AutomapperCarProfile()
    {
        CreateMap<IFormFile, ApplicationPhotoModel>()
            .ConvertUsing<FormFileToApplicationPhotoConverter>();
        
         // creation request -> DTO Add Car Domain
         CreateMap<AddCarRequest, AddedCarServicesDto>()
             .ForMember(dst => dst.Photo,
                 opt => opt.MapFrom(src => src.Photo));
         
         // patch request -> DTO Update Car Domain 
         CreateMap<PatchCarRequest, PatchCarServicesDto>()
             .ForMember(d => d.Mileage,
                 o => o.MapFrom(s => s.UsedParameters!.Mileage))
             .ForMember(d => d.CurrentOwner,
                 o => o.MapFrom(s => s.UsedParameters!.CurrentOwner));
         
         // Domain car -> created response
         CreateMap<ICar, CarResponse>()
             .ConvertUsing<DomainCarToResponseCarConverter>();
    }
}