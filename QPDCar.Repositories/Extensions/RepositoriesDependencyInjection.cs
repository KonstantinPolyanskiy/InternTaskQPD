using Microsoft.Extensions.DependencyInjection;
using QPDCar.Repositories.Repositories;
using QPDCar.Repositories.Repositories.PhotoDataRepositories;
using QPDCar.Services.Repositories;

namespace QPDCar.Repositories.Extensions;

public static class RepositoriesDependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IRefreshRepository, RefreshTokenRepository>();
        services.AddScoped<IBlackListAccessRepository, BlackListAccessTokenRepository>();
        
        services.AddScoped<ICarRepository, CarRepository>();
            
        services.AddScoped<IPhotoMetadataRepository, PhotoMetadataRepository>();
        services.AddScoped<IPhotoDataRepository, PostgresPhotoDataRepository>();
        
        return services;
    }
}