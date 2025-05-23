using AutoMapper;
using QPDCar.Models.BusinessModels.UserModels;
using QPDCar.Models.DtoModels.UserDtos;
using QPDCar.Models.StorageModels;

namespace QPDCar.UseCases.MapperProfiles;

public class ApplicationUserToUserSummary : Profile
{
    public ApplicationUserToUserSummary()
    {
        CreateMap<ApplicationUserEntity, UserSummary>()
            .ForMember(dest => dest.Login,
                opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.LastName,
                opt => opt.MapFrom(src => src.LastName ?? string.Empty))
            .ForMember(dest => dest.Roles,
                opt => opt.Ignore());
        
        CreateMap<DtoForCreateConsumer, DtoForCreateUser>()
            .ForMember(dest => dest.InitialRoles, opt => opt.Ignore());
    }
}