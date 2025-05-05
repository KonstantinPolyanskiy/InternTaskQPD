using Backend.App.Models.Business;
using Backend.App.Models.Commands;

namespace Backend.App.Services.UserService;

public interface IUserService
{
    public Task<ApplicationUser>CreateUserWithRoleAsync(CreateUserCommand cmd);
    public Task<ApplicationUser?> FindUserByLoginAsync(LoginUserCommand cmd);
    public Task<bool> CheckUserPasswordAsync(LoginUserCommand cmd);
}