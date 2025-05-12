using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Private.Storages.DbContexts;

namespace Private.Storages.Extensions;

public static class DbContextServiceCollectionExtension
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