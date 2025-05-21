using Microsoft.Extensions.DependencyInjection;
using QPDCar.UseCases.UseCases.ConsumerUseCases;
using QPDCar.UseCases.UseCases.EmployerUseCases;
using QPDCar.UseCases.UseCases.EmployerUseCases.AdminUseCases;
using QPDCar.UseCases.UseCases.UserUseCases;

namespace QPDCar.UseCases.Extensions;

public static class UseCasesDependencyExtension
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<CarConsumerUseCases>();
        services.AddScoped<ConsumerUseCases>();

        services.AddScoped<AdminUseCases>();
        
        services.AddScoped<CarEmployerUseCases>();
        services.AddScoped<PhotoEmployerUseCases>();

        services.AddScoped<UserUseCases>();
        
        return services;
    }
}