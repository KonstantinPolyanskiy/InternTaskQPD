using Microsoft.Extensions.DependencyInjection;
using QPDCar.Jobs.Jobs;
using Quartz;

namespace QPDCar.Jobs.Extensions;

public static class QuartzJobDependencyInjection
{
    public static IServiceCollection AddQuartzJobs(this IServiceCollection services)
    {
        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();
            
            var jk = new JobKey("car-no-photo");
            
            q.AddJob<PublishCarDontHavePhotoJob>(opts => opts.WithIdentity(jk));
            
            q.AddTrigger(opts => opts
                .ForJob(jk)
                .WithIdentity("car-no-photo-trigger")
                .WithCronSchedule("0 0/1 * * * ?",
                    x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow")))
            );
        });
        
        return services;
    }
}