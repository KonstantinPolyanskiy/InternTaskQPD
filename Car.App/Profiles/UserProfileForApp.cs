using AutoMapper;
using Car.App.Models.UserModels;
using Models.Bridge.Auth;

namespace Car.App.Profiles;

public class UserProfileForApp : Profile
{
    public UserProfileForApp()
    {
        CreateMap<ApplicationUser, UserRegistrationRequest>();
        CreateMap<UserRegistrationServiceResponse, ApplicationUser>();
    }
}