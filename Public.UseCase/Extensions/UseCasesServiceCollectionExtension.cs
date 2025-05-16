using Microsoft.Extensions.DependencyInjection;
using Private.Services.AuthServices;
using Private.Services.CarServices;
using Private.Services.EmailServices;
using Private.Services.ManagerServices;
using Private.Services.RoleServices;
using Private.Services.UserServices;
using Private.ServicesInterfaces;
using Public.UseCase.UseCases.AdminUseCases;
using Public.UseCase.UseCases.ConsumerUseCases;
using Public.UseCase.UseCases.ManagerUseCases;
using Public.UseCase.UseCases.UserUseCases;

namespace Public.UseCase.Extensions;

public static class UseCasesServiceCollectionExtension
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<IAuthTokenService, IdentityAuthTokenService>();
        services.AddScoped<IAuthService, IdentityAuthService>();
        services.AddScoped<IRoleService, IdentityRoleService>();
        services.AddScoped<IUserService, IdentityUserService>();

        services.AddScoped<ICarService, CarService>();
        services.AddScoped<IEmployerService, EmployerService>();
        
        services.AddScoped<IMailSenderService, StubEmailSenderService>();
        services.AddScoped<IEmailConfirmationService, EmailConfirmationService>();

        services.AddScoped<UserUseCase>();
        services.AddScoped<AdminUseCases>();
        services.AddScoped<CarEmployerUseCase>();
        services.AddScoped<PhotoEmployerUseCases>();
        services.AddScoped<ConsumerUseCases>();
        
        return services;
    }
}