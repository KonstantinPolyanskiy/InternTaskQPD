using AutoMapper;
using Backend.App.Models.Commands;
using Backend.App.Models.Business;
using Microsoft.AspNetCore.Identity;

namespace Backend.App.Services.UserService;

public class UserService(UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm) : IUserService
{
    public async Task<ApplicationUser> CreateUserWithRoleAsync(CreateUserCommand cmd)
    {
        if (await um.FindByNameAsync(cmd.Login) != null) throw new Exception($"User with login {cmd.Login} already exists.");
        if (await um.FindByEmailAsync(cmd.Email) != null) throw new Exception($"User with email {cmd.Email} already exists.");

        var applicationUser = new ApplicationUser
        {
            UserName = cmd.Login,
            Email = cmd.Email,
            FirstName = cmd.FirstName,
            LastName = cmd.LastName,
        };  
        
        var userResult = await um.CreateAsync(applicationUser, cmd.Password);
        if (!userResult.Succeeded) throw new Exception($"Failed to create user: {userResult.Errors.FirstOrDefault()?.Description}");

        if (!await rm.RoleExistsAsync(cmd.Role.ToString())) await rm.CreateAsync(new IdentityRole(cmd.Role.ToString()));
        
        await um.AddToRoleAsync(applicationUser, cmd.Role.ToString());

        return applicationUser;
    }

    public async Task<ApplicationUser?> FindUserByLoginAsync(LoginUserCommand cmd)
        => await um.FindByNameAsync(cmd.Login);

    public async Task<bool> CheckUserPasswordAsync(LoginUserCommand cmd)
    {
        var user = await um.FindByNameAsync(cmd.Login);
        if (user is null) return false;
        
        return await um.CheckPasswordAsync(user, cmd.Password);
    }
}