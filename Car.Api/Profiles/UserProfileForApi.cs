using AutoMapper;
using CarService.Models;
using Models.Bridge.Auth;

namespace CarService.Profiles;

public class UserProfileForApi : Profile
{
    public UserProfileForApi()
    {
        CreateMap<LoginRequest, UserLoginRequest>();
    }
}