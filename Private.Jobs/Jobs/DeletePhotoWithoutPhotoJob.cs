using Microsoft.EntityFrameworkCore;
using Private.StorageModels;
using Private.Storages.DbContexts;
using Private.Storages.Repositories.PhotoRepositories;
using Public.Models.ApplicationErrors;
using Public.Models.BusinessModels.StorageModels;
using Quartz;

namespace Private.Jobs.Jobs;

public class DeletePhotoWithoutPhotoJob(AppDbContext db, MinioPhotoRepository? minioPhotoRepository = null) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var photosWithoutCar = await db.PhotoMetadatas.Where(pm => pm.PhotoDataId == null)
            .Select(pm => new {pm.Id, pm.PhotoDataId})
            .ToListAsync();

        foreach (var photo in photosWithoutCar)
        {
            if (photo.PhotoDataId == null)
                continue;
            
            if (minioPhotoRepository != null)
            {
                var deletedResult = await minioPhotoRepository.DeletePhotoByIdAsync((Guid)photo.PhotoDataId, StorageTypes.None);
                if (deletedResult.IsSuccess)
                    continue;
            }
            
            db.Photos.Remove(new PhotoEntity {Id = (Guid)photo.PhotoDataId});
            await db.SaveChangesAsync();
        }
    }
}