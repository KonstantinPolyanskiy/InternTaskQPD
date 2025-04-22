using CarService.Models.User;
using Microsoft.AspNetCore.Identity;

namespace CarService.Daos.UserDao.EntityUserDao;

public class EntityUserDao(RoleManager<IdentityRole<int>> roleManager, UserManager<ApplicationUser> userManager) : IUserDao
{
    public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password)
    {
        return await userManager.CreateAsync(user, password); 
    }

    public async Task AddRoleToUserAsync(ApplicationUser user, string roleName)
    {
        if (!await userManager.IsInRoleAsync(user, roleName))
            await roleManager.CreateAsync(new IdentityRole<int>(roleName));
        
        await userManager.AddToRoleAsync(user, roleName);
    }

    public async Task<ApplicationUser?> FindUserByEmailAsync(string email)
    {
        return await userManager.FindByEmailAsync(email);
    }

    public async Task<ApplicationUser?> FindUserByIdAsync(string userId)
    {
        return await userManager.FindByIdAsync(userId);
    }

    public async Task<ApplicationUser?> FindUserByUsernameAsync(string username)
    {
        return await userManager.FindByNameAsync(username);
    }
}
