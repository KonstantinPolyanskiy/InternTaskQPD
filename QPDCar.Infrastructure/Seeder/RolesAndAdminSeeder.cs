using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QPDCar.Infrastructure.DbContexts;
using QPDCar.Models.BusinessModels.EmployerModels;
using QPDCar.Models.StorageModels;

namespace QPDCar.Infrastructure.Seeder;

public static class AppDbContextSeeder
{
    private const string AdminEmail    = "admin@mail.ru";
    private const string AdminUserName = "admin";
    private const string AdminPassword = "Password1!";

    public static async Task SeedAsync(
        IServiceProvider services,
        CancellationToken ct = default)
    {
        var logger = services.GetRequiredService<ILogger>();
        
        await using var db = services.GetRequiredService<AppDbContext>();

        var roleNames = Enum.GetNames<ApplicationRoles>();
        var existing  = await db.Roles.Select(r => r.Name!)
                                      .ToListAsync(ct);

        var newRoles = roleNames
            .Except(existing, StringComparer.OrdinalIgnoreCase)
            .Select(name => new IdentityRole
            {
                Id             = Guid.NewGuid().ToString(),
                Name           = name,
                NormalizedName = name.ToUpperInvariant(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            })
            .ToArray();

        if (newRoles.Length > 0)
        {
            db.Roles.AddRange(newRoles);
            logger.LogInformation("Добавлены роли: {Roles}", string.Join(", ", newRoles.Select(r => r.Name)));
        }

        var admin = await db.Users.FirstOrDefaultAsync(
            u => u.UserName == AdminUserName, ct);

        if (admin is null)
        {
            var hasher = new PasswordHasher<ApplicationUserEntity>();

            admin = new ApplicationUserEntity
            {
                Id = Guid.NewGuid().ToString(),
                UserName         = AdminUserName,
                NormalizedUserName = AdminUserName.ToUpperInvariant(),
                Email            = AdminEmail,
                NormalizedEmail  = AdminEmail.ToUpperInvariant(),
                EmailConfirmed   = true,
                SecurityStamp    = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                FirstName        = "Super User Name",
                PasswordHash     = hasher.HashPassword(null!, AdminPassword)
            };

            db.Users.Add(admin);
            logger.LogInformation("Создан супер-пользователь {User}", AdminUserName);
        }

        var adminRoleIds = await db.UserRoles.Where(ur => ur.UserId == admin.Id)
                                             .Select(ur => ur.RoleId)
                                             .ToListAsync(ct);

        var allRoleIds = await db.Roles.Where(r => roleNames.Contains(r.Name!))
                                       .Select(r => r.Id)
                                       .ToListAsync(ct);

        var missingLinks = allRoleIds.Except(adminRoleIds)
                                     .Select(rid => new IdentityUserRole<string>
                                     {
                                         UserId = admin.Id,
                                         RoleId = rid
                                     })
                                     .ToArray();

        if (missingLinks.Length > 0)
        {
            db.UserRoles.AddRange(missingLinks);
            logger.LogInformation("Администратору добавлены роли ({Count})", missingLinks.Length);
        }

        if (db.ChangeTracker.HasChanges())
            await db.SaveChangesAsync(ct);
    }
}