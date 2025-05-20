using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using QPDCar.Infrastructure.DbContexts;

namespace QPDCar.Infrastructure.DbContextFactory;
 
/// <summary> Фабрика DbContext для корректных миграций </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory()
                       + Path.DirectorySeparatorChar
                       + ".."
                       + Path.DirectorySeparatorChar
                       + "QPDCar.Api";
        
        var cfg = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
        var conn = cfg.GetConnectionString("Default");
        
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        
        builder.UseNpgsql(conn, sql =>
            sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
        
        return new AppDbContext(builder.Options);
    }
}