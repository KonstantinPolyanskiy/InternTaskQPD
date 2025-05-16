using Microsoft.EntityFrameworkCore;
using Private.Jobs.Models;
using Private.ServicesInterfaces;
using Private.StorageModels;
using Private.Storages.DbContexts;
using Quartz;

namespace Private.Jobs.Jobs;

public class PublishCarDontHavePhotoJob(AppDbContext db, IEventPublisher publisher) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var pairs = await db.Cars
            .Where(c => c.PhotoMetadataId == null)                   
            .Join(
                db.ApplicationUsers
                    .Where(u => u.EmailConfirmed                      
                                && !string.IsNullOrEmpty(u.Email)),      
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
        await publisher.PublishAsync(payload);
    }
}