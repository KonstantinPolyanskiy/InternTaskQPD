using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Backend.Dal;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory()
                       + Path.DirectorySeparatorChar
                       + ".."
                       + Path.DirectorySeparatorChar
                       + "Backend.Api";
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

public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory()
                       + Path.DirectorySeparatorChar
                       + ".."
                       + Path.DirectorySeparatorChar
                       + "Backend.Api";
        var cfg = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
        var conn = cfg.GetConnectionString("Default");
        var builder = new DbContextOptionsBuilder<AuthDbContext>();
        builder.UseNpgsql(conn, sql => 
            sql.MigrationsAssembly(typeof(AuthDbContext).Assembly.FullName));
        return new AuthDbContext(builder.Options);
    }
}

