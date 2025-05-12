using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Private.Storages.DbContexts;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory()
                       + Path.DirectorySeparatorChar
                       + ".."
                       + Path.DirectorySeparatorChar
                       + "Public.Api";
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
