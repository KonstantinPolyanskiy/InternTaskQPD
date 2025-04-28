using Car.App.Repositories;
using Car.App.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Car.App.Extensions;

public static class ServiceCollectionExtension
{
    /// <summary>
    /// Добавляет сервисы бизнес-логики в зависимости
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddApp(this IServiceCollection services)
    {
        services.AddScoped<CarService>();
        return services;
    }
}
