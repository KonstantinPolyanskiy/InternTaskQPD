using Backend.App.Services.CarService;
using Backend.App.Services.PhotoService;
using Backend.App.Services.TokenService;
using Backend.App.Services.UserService;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.App.Extensions;

public static class ServiceCollectionExtension
{
    /// <summary>
    /// Добавляет сервисы бизнес-логики в зависимости
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddAllNeedServices(this IServiceCollection services)
    {
        services.AddScoped<InternalPhotoService>();
        services.AddScoped<CarService>();
        services.AddScoped<ITokenService, SimpleTokenService>();
        services.AddScoped<IUserService, UserService>();
        
        return services;
    }
}
