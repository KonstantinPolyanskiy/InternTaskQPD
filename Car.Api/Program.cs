using Car.App.Extensions;
using Car.App.Profiles;
using Car.App.Repositories;
using Car.App.Services;
using Car.App.Services.TokenService;
using Car.Dal;
using Car.Dal.Repository.EntityFrameworkRepository;
using CarService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models.Bridge.Auth;

var builder = WebApplication.CreateBuilder(args);

// TODO: логгер

// Data Layer
builder.Services.AddDbContext<AppDbContext>((_, opt) =>
{
    var conn = "Host=localhost;Port=5313;Database=main_db;Username=admin;Password=password";
    opt.UseNpgsql(conn, npg =>
    {
        npg.EnableRetryOnFailure();                               
    });
});
builder.Services.AddDbContext<AuthDbContext>((_, opt) =>
{
    var conn = "Host=localhost;Port=5313;Database=main_db;Username=admin;Password=password";
    opt.UseNpgsql(conn, npg =>
    {
        npg.EnableRetryOnFailure();                               
    });
});
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<ICarRepository, PostgresCarRepository>();
builder.Services.AddScoped<IPhotoRepository, PostgresPhotoRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, PostgresRefreshTokenRepository>();

// Jwt settings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings")
);

// Domain Layer
builder.Services.AddAutoMapper(typeof(CarProfileForApp).Assembly);
builder.Services.AddScoped<PhotoProcessor>();
builder.Services.AddScoped<ITokenService, SimpleTokenService>();
builder.Services.AddApp();

// Api Layer
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Build application
var app = builder.Build();

// Roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var roles = new[] { "admin", "manager", "client" };

    foreach (var roleName in roles)
    {
        var exists = await roleManager.RoleExistsAsync(roleName);
        if (!exists)
        {
            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                throw new Exception($"Не удалось создать роль '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}

// Migrations
using (var scope = app.Services.CreateScope())
{
    var serviceDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    serviceDb.Database.Migrate();
    
    var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    authDb.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();