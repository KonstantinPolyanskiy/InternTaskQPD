using AutoMapper;
using Backend.Api.Models;
using Backend.Api.Models.Requests;
using Backend.Api.Models.Responses;
using Backend.App.Models.Business;
using Backend.App.Models.Commands;

namespace Backend.Api.Profiles;

public class UserProfileForApi : Profile
{
    public UserProfileForApi()
    {
        CreateMap<UserRegistrationRequest, CreateUserCommand>();

        CreateMap<ApplicationUser, CreateUserResponse>()
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Login, opt => opt.MapFrom(src => src.UserName));
        
        CreateMap<LoginRequest, LoginUserCommand>();
        
        CreateMap<ApplicationUser, GenerateTokenPairCommand>()
            .ForMember(dest => dest.Password, opt => opt.Ignore());
        
        CreateMap<TokenPair, TokenPairResponse>();
        CreateMap<RefreshTokenPairRequest, RefreshTokenPairCommand>();
    }
}