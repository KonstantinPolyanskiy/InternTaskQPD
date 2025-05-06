using AutoMapper;
using Backend.App.Models.Commands;
using Backend.App.Models.Business;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Backend.App.Services.UserService;

public class UserService(ILogger<UserService> log,
    UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm) : IUserService
{
    public async Task<ApplicationUser> CreateUserWithRoleAsync(CreateUserCommand cmd)
    {
        log.LogInformation("Попытка создать пользователя с логином {login}", cmd.Login);
        log.LogDebug("Данные для создания пользователя - {data}", cmd);
        
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
        
        log.LogInformation("Пользователь с логином {login} успешно создан", applicationUser.UserName);
        log.LogDebug("Созданный пользователь - {@data}", applicationUser);

        return applicationUser;
    }

    public async Task<ApplicationUser?> FindUserByLoginAsync(LoginUserCommand cmd)
    {
        log.LogInformation("Попытка найти пользователя с логином {login}", cmd.Login);
        
        var user = await um.FindByNameAsync(cmd.Login);
        if (user == null) return null;
        
        log.LogInformation("Пользователь с логином {login} успешно найден", user.UserName);
        return user;
    }

    public async Task<bool> CheckUserPasswordAsync(LoginUserCommand cmd)
    {
        log.LogInformation("Попытка проверить пароль пользователя {login}", cmd.Login);
        
        var user = await um.FindByNameAsync(cmd.Login);
        if (user is null) return false;
        
        var result = await um.CheckPasswordAsync(user, cmd.Password);
        
        log.LogInformation("Введенный пароль для пользователя {login} - {correct}", user.UserName, result ? "верен" : "не верен");
        
        return result;
    }
}