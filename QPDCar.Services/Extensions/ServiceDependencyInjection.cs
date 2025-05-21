using Microsoft.Extensions.DependencyInjection;
using QPDCar.ServiceInterfaces;
using QPDCar.ServiceInterfaces.MailServices;
using QPDCar.ServiceInterfaces.UserServices;
using QPDCar.Services.Services;
using QPDCar.Services.Services.UserServices;

namespace QPDCar.Services.Extensions;

public static class ServiceDependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ICarService, CarService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IEmployerService, EmployerService>();
        services.AddScoped<IAuthTokenService, AuthTokenService>();
        services.AddScoped<IMailConfirmationService, MailConfirmationService>();
        
        return services;
    }
}