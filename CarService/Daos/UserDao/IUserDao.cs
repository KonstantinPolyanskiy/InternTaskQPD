using CarService.Models.User;
using Microsoft.AspNetCore.Identity;

namespace CarService.Daos.UserDao;

public interface IUserDao
{
    Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);
    Task AddRoleToUserAsync(ApplicationUser user, string roleName);
    
    Task<ApplicationUser?> FindUserByEmailAsync(string email);
    Task<ApplicationUser?> FindUserByIdAsync(string userId);
    Task<ApplicationUser?> FindUserByUsernameAsync(string username);
}