using AutoMapper;
using QPDCar.Models.BusinessModels.EmployerModels;
using QPDCar.Models.StorageModels;

namespace QPDCar.Services.MapperProfiles;

public class UserMapperProfile : Profile
{
    public UserMapperProfile()
    {
        CreateMap<ApplicationUserEntity, DomainEmployer>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(src => Guid.Parse(src.Id)))
                
            .ForMember(dest => dest.Login,
                opt => opt.MapFrom(src => src.UserName))
                
            .ForMember(dest => dest.LastName,
                opt => opt.MapFrom(src => src.LastName ?? string.Empty))
                
            .ForMember(dest => dest.Roles,
                opt => opt.Ignore());
    }
}