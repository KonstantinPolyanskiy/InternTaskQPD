using Backend.Api.Extensions;
using Backend.Api.Models.Responses;
using Backend.Api.Processors;
using Backend.Api.Profiles;
using Backend.App.Extensions;
using Backend.App.Models.Business;
using Backend.App.Profiles;
using Backend.App.Repositories;
using Backend.App.Services.TokenService;
using Backend.App.Services.UserService;
using Backend.Dal;
using Backend.Dal.Repository;
using Backend.Dal.Repository.MinioRepository;
using Backend.Dal.Repository.PostgresRepository;
using Enum.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Minio;
using Npgsql;
using Settings.Common;

var builder = WebApplication.CreateBuilder(args);

// TODO: logging

builder.Services.AddHttpContextAccessor();

#region Minio

builder.Services.Configure<MinioSettings>(builder.Configuration.GetSection("MinioSettings"));

builder.Services.AddSingleton<IMinioClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MinioSettings>>();
    var cb = new MinioClient()
        .WithEndpoint(settings.Value.Endpoint, settings.Value.Port)
        .WithCredentials(settings.Value.AccessKey, settings.Value.SecretKey);
    
    if (settings.Value.UseSSL) cb = cb.WithSSL();
    return cb.Build();
});

#endregion

#region Postgres

var defaultPgConn = builder.Configuration.GetConnectionString("Default");

#endregion

#region DbContexts

builder.Services.AddDbContext<AppDbContext>((_, opt) =>
{
    opt.UseNpgsql(defaultPgConn, npg =>
    {
        npg.EnableRetryOnFailure();     
        npg.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
    });
});
builder.Services.AddDbContext<AuthDbContext>((_, opt) =>
{
    opt.UseNpgsql(defaultPgConn, npg =>
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

builder.Services.AddScoped<IPhotoMetadataRepository, PhotoMetadataPostgresRepository>();
builder.Services.AddScoped<PostgresPhotoRepository>();
builder.Services.AddScoped<PhotoMinioRepository>();

builder.Services.AddScoped<IPhotoRepository, PhotoRepository>();

builder.Services.AddScoped<IRefreshTokenRepository, PostgresRefreshTokenRepository>();
builder.Services.AddScoped<IBlacklistTokenRepository, PostgresBlacklistTokenRepository>();

#endregion

#region Services

builder.Services.AddAutoMapper(typeof(CarProfileForApp).Assembly);
builder.Services.AddAutoMapper(typeof(UserProfileForApp).Assembly);

builder.Services.AddSingleton<PhotoProcessor>();
builder.Services.AddAllNeedServices();

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

    var roleNames = System.Enum.GetNames<ApplicationUserRole>();

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