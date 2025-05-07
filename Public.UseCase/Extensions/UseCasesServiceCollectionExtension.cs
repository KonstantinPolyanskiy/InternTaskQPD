using Microsoft.Extensions.DependencyInjection;
using Private.Services.UserServices;
using Private.ServicesInterfaces;

namespace Public.UseCase.Extensions;

public static class UseCasesServiceCollectionExtension
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<IUserService, IdentityUserService>();
        return services;
    }
}