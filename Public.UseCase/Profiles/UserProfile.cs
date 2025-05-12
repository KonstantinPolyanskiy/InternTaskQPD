using AutoMapper;
using Private.StorageModels;
using Public.Models.DtoModels.UserDtoModels;

namespace Public.UseCase.Profiles;

public class UserMapProfile : Profile
{
    public UserMapProfile()
    {
        CreateMap<DataForCreateUser, ApplicationUserEntity>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Login))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))

            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshTokens, opt => opt.Ignore())
            .ForMember(dest => dest.EmailConfirmationToken, opt => opt.Ignore());
    }
}