using Microsoft.AspNetCore.Identity;
using Private.StorageModels;
using Public.Models.CommonModels;

namespace Public.Api.Seeder;

public static class IdentitySeeder
{
    private const string AdminEmail    = "admin@mail.ru";
    private const string AdminUserName = "admin";
    private const string AdminPassword = "Password1!";         

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUserEntity>>();
        
        foreach (ApplicationUserRole role in Enum.GetValues(typeof(ApplicationUserRole)))
        {
            var name = role.ToString();
            if (!await roleManager.RoleExistsAsync(name))
            {
                await roleManager.CreateAsync(new IdentityRole { Name = name, NormalizedName = name.ToUpperInvariant() });
            }
        }
        
        var super = await userManager.FindByNameAsync(AdminUserName);
        if (super is null)
        {
            super = new ApplicationUserEntity
            {
                UserName = AdminUserName,
                Email = AdminEmail,
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                FirstName = "Super User Name"
            };

            var create = await userManager.CreateAsync(super, AdminPassword);
            if (!create.Succeeded)
                throw new Exception("Не удалось создать суперпользователя: " +
                                    string.Join("; ", create.Errors.Select(e => e.Description)));
        }

        var allRoles = Enum.GetNames(typeof(ApplicationUserRole));
        var inRoles  = await userManager.GetRolesAsync(super);

        var toAdd = allRoles.Except(inRoles).ToArray();
        if (toAdd.Length > 0)
            await userManager.AddToRolesAsync(super, toAdd);
    }
}