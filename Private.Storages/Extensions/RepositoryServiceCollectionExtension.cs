using Microsoft.Extensions.DependencyInjection;
using Private.Services.Repositories;
using Private.Storages.Repositories.BlackListAccessTokenRepository;
using Private.Storages.Repositories.CarRepository;
using Private.Storages.Repositories.EmailConfirmationTokenRepository;
using Private.Storages.Repositories.PhotoMetadataRepository;
using Private.Storages.Repositories.PhotoRepositories.PostgresPhotoRepository;
using Private.Storages.Repositories.RefreshTokenRepository;

namespace Private.Storages.Extensions;

public static class RepositoryServiceCollectionExtension
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IEmailConfirmationTokenRepository, EmailConfirmationTokenRepository>();
        
        services.AddScoped<IRefreshTokenRepository, PostgresRefreshTokenRepository>();
        services.AddScoped<IBlackListAccessTokenRepository, BlackListAccessTokenRepository>();
        
        services.AddScoped<ICarRepository, CarRepository>();
            
        services.AddScoped<IPhotoMetadataRepository, PhotoMetadataRepository>();
        services.AddScoped<IPhotoRepository, PhotoPostgresRepository>();
        
        return services;
    }
}