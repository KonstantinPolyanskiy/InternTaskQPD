using AutoMapper;
using Car.App.Models.UserModels;
using Microsoft.AspNetCore.Identity;
using Models.Bridge.Auth;

namespace Car.App.Services.UserService;

public class UserService(IMapper mapper,
    UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm) : IUserService
{
    public async Task<UserRegistrationServiceResponse> CreateUserWithRoleAsync(UserRegistrationRequest request, ApplicationUserRole applicationUserRole)
    {
        if (await um.FindByNameAsync(request.Login) != null)
            throw new Exception($"User with login {request.Login} already exists.");
        if (await um.FindByEmailAsync(request.Email) != null) 
            throw new Exception($"User with email {request.Email} already exists.");

        var applicationUser = mapper.Map<ApplicationUser>(request);
        
        var userResult = await um.CreateAsync(applicationUser, request.Password);
        if (!userResult.Succeeded)
            throw new Exception($"Failed to create user: {userResult.Errors.FirstOrDefault()?.Description}");

        if (!await rm.RoleExistsAsync(applicationUserRole.ToString())) 
            await rm.CreateAsync(new IdentityRole(applicationUserRole.ToString()));
        
        await um.AddToRoleAsync(applicationUser, applicationUserRole.ToString());
        
        return mapper.Map<UserRegistrationServiceResponse>(userResult);
    }
}