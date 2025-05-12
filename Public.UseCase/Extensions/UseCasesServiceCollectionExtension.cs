using Microsoft.Extensions.DependencyInjection;
using Private.Services.CarServices;
using Private.Services.EmailSenderServices;
using Private.Services.PhotoServices;
using Private.Services.RoleServices;
using Private.Services.TokenServices;
using Private.Services.UserServices;
using Private.ServicesInterfaces;
using Public.UseCase.UseCases.AdminUseCases;
using Public.UseCase.UseCases.CarUseCases;
using Public.UseCase.UseCases.ConsumerUseCases;
using Public.UseCase.UseCases.UserUseCases;

namespace Public.UseCase.Extensions;

public static class UseCasesServiceCollectionExtension
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<IRoleService, IdentityRoleService>();
        services.AddScoped<IUserService, IdentityUserService>();

        services.AddScoped<ICarService, CarService>();
        services.AddScoped<IPhotoService, PhotoService>();
        
        services.AddScoped<ITokenService, HandmadeTokenService>();

        services.AddScoped<IMailSenderService, StubEmailSenderService>();

        services.AddScoped<UserUseCase>();
        services.AddScoped<AdminUseCases>();
        services.AddScoped<CarUseCase>();
        services.AddScoped<ConsumerUseCases>();
        
        return services;
    }
}