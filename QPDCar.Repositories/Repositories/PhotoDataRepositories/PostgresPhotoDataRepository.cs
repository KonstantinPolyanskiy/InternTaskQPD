using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QPDCar.Infrastructure.DbContexts;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.StorageModels;
using QPDCar.Repositories.ErrorHelpers;
using QPDCar.Services.Repositories;

namespace QPDCar.Repositories.Repositories.PhotoDataRepositories;

public class PostgresPhotoDataRepository(AppDbContext db, ILogger<PostgresPhotoDataRepository> logger) : IPhotoDataRepository
{
    private const string EntityName = "Photo Metadata";

    public async Task<ApplicationExecuteResult<PhotoDataEntity>> SaveAsync(PhotoDataEntity photoData)
    {
        try
        {
            await db.PhotoData.AddAsync(photoData);
            await db.SaveChangesAsync();
            
            return ApplicationExecuteResult<PhotoDataEntity>.Success(photoData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteResult<PhotoDataEntity>.Failure(ErrorHelper.PrepareNotSavedError(EntityName));
        }
    }

    public async Task<ApplicationExecuteResult<PhotoDataEntity>> ByIdAsync(Guid photoDataId)
    {
        try
        {
            var entity = await db.PhotoData.FirstOrDefaultAsync(x => x.Id == photoDataId);
            if (entity == null)
                return ApplicationExecuteResult<PhotoDataEntity>.Failure(ErrorHelper.PrepareNotFoundErrorSingle(EntityName));
            
            return ApplicationExecuteResult<PhotoDataEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteResult<PhotoDataEntity>.Failure(ErrorHelper.PrepareNotFoundErrorSingle(EntityName));
        }
    }
}