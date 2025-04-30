using System.Text;
using Car.App.Extensions;
using Car.App.Models.UserModels;
using Car.App.Profiles;
using Car.App.Repositories;
using Car.App.Services;
using Car.App.Services.TokenService;
using Car.App.Services.UserService;
using Car.Dal;
using Car.Dal.Repository.PostgresRepository;
using CarService.Extensions;
using CarService.Profiles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models.Bridge.Auth;
using Models.Shared.User;

var builder = WebApplication.CreateBuilder(args);

// TODO: logging

#region DbContexts
builder.Services.AddDbContext<AppDbContext>((_, opt) =>
{
    var conn = "Host=localhost;Port=5313;Database=main_db;Username=admin;Password=password";
    opt.UseNpgsql(conn, npg =>
    {
        npg.EnableRetryOnFailure();     
        npg.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
    });
});
builder.Services.AddDbContext<AuthDbContext>((_, opt) =>
{
    var conn = "Host=localhost;Port=5313;Database=main_db;Username=admin;Password=password";
    opt.UseNpgsql(conn, npg =>
    {
        npg.EnableRetryOnFailure();
        npg.MigrationsAssembly(typeof(AuthDbContext).Assembly.FullName);
    });
});
#endregion

#region Identity

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

#endregion

#region Repositories

builder.Services.AddScoped<ICarRepository, PostgresCarRepository>();
builder.Services.AddScoped<IPhotoRepository, PostgresPhotoRepository>();

builder.Services.AddScoped<IRefreshTokenRepository, PostgresRefreshTokenRepository>();
builder.Services.AddScoped<IBlacklistTokenRepository, PostgresBlacklistTokenRepository>();

#endregion

#region Services

builder.Services.AddAutoMapper(typeof(CarProfileForApp).Assembly);
builder.Services.AddAutoMapper(typeof(UserProfileForApp).Assembly);
builder.Services.AddScoped<PhotoProcessor>();
builder.Services.AddScoped<ITokenService, SimpleTokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddApp();

#endregion

#region JwtSettings

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings")
);

#endregion

#region Api

builder.Services.AddAutoMapper(typeof(UserProfileForApi).Assembly);
builder.Services.AddAutoMapper(typeof(CarProfileForApi).Assembly);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

#region SwaggerSecurity

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerJwtScheme();
    

#endregion

#endregion


var app = builder.Build();



#region Migrations

using (var scope = app.Services.CreateScope())
{
    var serviceDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    serviceDb.Database.Migrate();
    
    var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    authDb.Database.Migrate();
}

#endregion

#region Roles

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var roleNames = Enum.GetNames<ApplicationUserRole>();

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                throw new Exception(
                    $"Не удалось создать роль '{roleName}': " +
                    string.Join(", ", result.Errors.Select(e => e.Description))
                );
            }
        }
    }
}

#endregion

#region AppUse's

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

#endregion

app.Run();