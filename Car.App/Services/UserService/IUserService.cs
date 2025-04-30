using Car.App.Models.UserModels;
using Models.Bridge.Auth;

namespace Car.App.Services.UserService;

public interface IUserService
{
    public Task<UserRegistrationServiceResponse>CreateUserWithRoleAsync(UserRegistrationRequest request, ApplicationUserRole role);
    Task<LoginServiceResponse> FindUserByLoginAndCheckPassword(UserLoginRequest request);
}