using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QPDCar.Infrastructure.DbContexts;

namespace QPDCar.Infrastructure.Extensions;

public static class DbContextDependencyInjection
{
    public static IServiceCollection AddDbContexts(this IServiceCollection services, string conn)
    {
        services.AddDbContext<AppDbContext>((_, opt) =>
        {
            opt.UseNpgsql(conn, npg =>
            {
                npg.EnableRetryOnFailure();     
                npg.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            });
        });
        
        return services;
    }
}