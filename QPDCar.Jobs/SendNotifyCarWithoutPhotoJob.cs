using Microsoft.EntityFrameworkCore;
using QPDCar.Infrastructure.DbContexts;
using QPDCar.Models.ApplicationModels.Events;
using QPDCar.ServiceInterfaces.Publishers;
using Quartz;

namespace QPDCar.Jobs;

public class PublishCarDontHavePhotoJob(AppDbContext db, INotificationPublisher publisher) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var pairs = await db.Cars
            .Where(c => c.PhotoMetadataId == null)                   
            .Join(
                db.ApplicationUser
                    .Where(u => u.Email != null),      
                car  => car.ResponsiveManagerId.ToString(),                   
                user => user.Id,                                    
                (car, user) => new                                 
                {
                    CarId        = car.Id,
                    ManagerEmail = user.Email!
                })
            .ToListAsync();                                      

        if (pairs.Count == 0) return;

        var payload = pairs.Select(c => new { c.CarId, c.ManagerEmail }).ToList();

        foreach (var pair in payload)
        {
            await publisher.NotifyAsync(new EmailNotificationEvent
            {
                MessageId = Guid.NewGuid(),
                To = pair.ManagerEmail,
                Subject = "Машина без фото",
                BodyHtml = $"Машина {pair.CarId} все еще без фото"
            });
        }
    }
}