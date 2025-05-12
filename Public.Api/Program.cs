using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Minio;
using Private.StorageModels;
using Private.Storages.DbContexts;
using Private.Storages.Extensions;
using Public.Api.Extensions;
using Public.Api.Middlewares;
using Public.Api.Seeder;
using Public.Models.CommonModels;
using Public.UseCase.Extensions;
using Serilog;
using Serilog.Events;
using Settings.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();


#region Minio

builder.Services.Configure<MinioSettings>(builder.Configuration.GetSection("MinioSettings"));

builder.Services.AddSingleton<IMinioClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MinioSettings>>();
    var cb = new MinioClient()
        .WithEndpoint(settings.Value.Endpoint, settings.Value.Port)
        .WithCredentials(settings.Value.AccessKey, settings.Value.SecretKey);
    
    if (settings.Value.UseSSL) cb = cb.WithSSL();
    return cb.Build();
});

#endregion

#region Logger

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("BackendCarService", builder.Environment.ApplicationName)
    .CreateLogger();

builder.Host.UseSerilog();

#endregion

var defaultPgConn = builder.Configuration.GetConnectionString("Default");


builder.Services.AddDbContexts(defaultPgConn!);

builder.Services.AddRepositories();

builder.Services.AddUseCases();

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings")
);

builder.Services.AddIdentity<ApplicationUserEntity, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerJwtScheme();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var serviceDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    serviceDb.Database.Migrate();
}

await IdentitySeeder.SeedAsync(app.Services);

app.UseMiddleware<CorrelationIdMiddleware>();      
app.UseSerilogRequestLogging(opts =>
{
    opts.GetLevel = (ctx, _, ex) =>
        ex != null || ctx.Response.StatusCode >= 500
            ? LogEventLevel.Error
            : LogEventLevel.Information;
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();