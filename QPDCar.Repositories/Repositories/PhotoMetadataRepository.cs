using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QPDCar.Infrastructure.DbContexts;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.StorageModels;
using QPDCar.Repositories.ErrorHelpers;
using QPDCar.Services.Repositories;

namespace QPDCar.Repositories.Repositories;

public class PhotoMetadataRepository(AppDbContext db, ILogger<PhotoMetadataRepository> logger) : IPhotoMetadataRepository
{
    private const string EntityName = "Photo Metadata";
    
    public async Task<ApplicationExecuteResult<PhotoMetadataEntity>> SaveAsync(PhotoMetadataEntity entity)
    {
        try
        {
            await db.PhotoMetadata.AddAsync(entity);
            await db.SaveChangesAsync();
            
            return ApplicationExecuteResult<PhotoMetadataEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteResult<PhotoMetadataEntity>.Failure(ErrorHelper.PrepareNotSavedError(EntityName));
        }
    }

    public async Task<ApplicationExecuteResult<PhotoMetadataEntity>> ByIdAsync(int id)
    {
        try
        {
            var entity = await db.PhotoMetadata.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return ApplicationExecuteResult<PhotoMetadataEntity>.Failure(ErrorHelper.PrepareNotFoundErrorSingle(EntityName));
            
            return ApplicationExecuteResult<PhotoMetadataEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteResult<PhotoMetadataEntity>.Failure(ErrorHelper.PrepareNotFoundErrorSingle(EntityName));
        }
    }

    public async Task<ApplicationExecuteResult<PhotoMetadataEntity>> ByDataIdAsync(Guid photoDataId)
    {
        try
        {
            var entity = await db.PhotoMetadata.FirstOrDefaultAsync(x => x.PhotoDataId == photoDataId);
            if (entity == null)
                return ApplicationExecuteResult<PhotoMetadataEntity>.Failure(ErrorHelper.PrepareNotFoundErrorSingle(EntityName));
            
            return ApplicationExecuteResult<PhotoMetadataEntity>.Success(entity);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteResult<PhotoMetadataEntity>.Failure(ErrorHelper.PrepareNotFoundErrorSingle(EntityName));
        }
    }
}