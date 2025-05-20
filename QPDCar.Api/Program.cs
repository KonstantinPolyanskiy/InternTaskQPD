using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Minio;
using QPDCar.Api.Extensions;
using QPDCar.Api.Middlewares;
using QPDCar.Api.Seeder;
using QPDCar.Infrastructure.DbContexts;
using QPDCar.Infrastructure.Extensions;
using QPDCar.Infrastructure.Publishers;
using QPDCar.Jobs;
using QPDCar.Models.ApplicationModels.Settings;
using QPDCar.Models.StorageModels;
using QPDCar.Repositories.Extensions;
using QPDCar.ServiceInterfaces.Publishers;
using QPDCar.Services.Extensions;
using QPDCar.UseCases.Models.UserModels;
using QPDCar.UseCases.UseCases.ConsumerUseCases;
using QPDCar.UseCases.UseCases.EmployerUseCases;
using QPDCar.UseCases.UseCases.EmployerUseCases.AdminUseCases;
using QPDCar.UseCases.UseCases.UserUseCases;
using Quartz;
using RabbitMQ.Client;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtAuthSettings>(
    builder.Configuration.GetSection("JwtSettings")
);

builder.Services.Configure<MinioSettings>(
    builder.Configuration.GetSection("MinioSettings")
);

builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings")
);

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

builder.Services.AddSingleton<IConnection>(sp =>
{
    var cfg     = sp.GetRequiredService<IConfiguration>();
    var factory = new ConnectionFactory
    {
        HostName = cfg["Rabbit:Host"],
        UserName = cfg["Rabbit:User"],
        Password = cfg["Rabbit:Pass"],
    };

    return factory.CreateConnectionAsync("api-publisher").Result;
});

builder.Services.AddScoped<INotificationPublisher, RabbitPublisher>();

var defaultPgConn = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContexts(defaultPgConn!);

builder.Services.AddMailSender(builder.Configuration.GetSection("SmtpSettings"));

builder.Services.AddRepositories();

builder.Services.AddServices();

builder.Services.AddScoped<CarConsumerUseCases>();
builder.Services.AddScoped<ConsumerUseCases>();

builder.Services.AddScoped<CarEmployerUseCases>();
builder.Services.AddScoped<PhotoEmployerUseCases>();

builder.Services.AddScoped<AdminUseCases>();
builder.Services.AddScoped<UserUseCases>();

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    var jobKey = new JobKey("car-no-photo");

    q.AddJob<PublishCarDontHavePhotoJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity("car-no-photo-trigger")
            .WithCronSchedule("0 0/1 * * * ?",
                x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow")))
    );
});

builder.Services.AddQuartzHostedService(opt =>
{
    opt.WaitForJobsToComplete = true;      
});

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