using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Private.Services.Repositories;
using Private.StorageModels;
using Private.Storages.DbContexts;
using Private.Storages.ErrorHelpers;
using Public.Models.CommonModels;

namespace Private.Storages.Repositories.PhotoMetadataRepository;

public class PhotoMetadataRepository(AppDbContext db, ILogger<PhotoMetadataRepository> logger) : IPhotoMetadataRepository
{
    private const string EntityName = "PhotoMetadata";
    
    public async Task<ApplicationExecuteLogicResult<PhotoMetadataEntity>> SavePhotoMetadataAsync(PhotoMetadataEntity entity)
    {
        try
        {
            await db.PhotoMetadatas.AddAsync(entity);
            await db.SaveChangesAsync();
            
            return ApplicationExecuteLogicResult<PhotoMetadataEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<PhotoMetadataEntity>.Failure(ErrorHelper.PrepareStorageException(EntityName));
        }
    }

    public async Task<ApplicationExecuteLogicResult<PhotoMetadataEntity>> GetPhotoMetadataByIdAsync(int id)
    {
        try
        {
            var entity = await db.PhotoMetadatas.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return ApplicationExecuteLogicResult<PhotoMetadataEntity>.Failure(ErrorHelper.PrepareNotFoundError(EntityName));
            
            return ApplicationExecuteLogicResult<PhotoMetadataEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<PhotoMetadataEntity>.Failure(ErrorHelper.PrepareStorageException(EntityName));
        }
    }

    public async Task<ApplicationExecuteLogicResult<PhotoMetadataEntity>> GetPhotoMetadataByPhotoIdAsync(Guid photoId)
    {
        try
        {
            var entity = await db.PhotoMetadatas.FirstOrDefaultAsync(x => x.PhotoDataId == photoId);
            if (entity == null)
                return ApplicationExecuteLogicResult<PhotoMetadataEntity>.Failure(ErrorHelper.PrepareNotFoundError(EntityName));
            
            return ApplicationExecuteLogicResult<PhotoMetadataEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<PhotoMetadataEntity>.Failure(ErrorHelper.PrepareStorageException(EntityName));
        }
    }

    public async Task<ApplicationExecuteLogicResult<PhotoMetadataEntity>> RewritePhotoMetadataAsync(PhotoMetadataEntity entity)
    {
        try
        {
            var exist = await db.PhotoMetadatas.FirstOrDefaultAsync(e => e.Id == entity.Id);
            if (exist == null)
                return ApplicationExecuteLogicResult<PhotoMetadataEntity>.Failure(ErrorHelper.PrepareNotFoundError(EntityName));

            db.Entry(exist).CurrentValues.SetValues(entity);
            await db.SaveChangesAsync();

            return ApplicationExecuteLogicResult<PhotoMetadataEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<PhotoMetadataEntity>.Failure(ErrorHelper.PrepareStorageException(EntityName));
        }
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> DeletePhotoMetadataAsync(int id)
    {
        try
        {
            var entity = await db.PhotoMetadatas.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return ApplicationExecuteLogicResult<Unit>.Failure(ErrorHelper.PrepareNotFoundError(EntityName));
            
            db.PhotoMetadatas.Remove(entity);
            await db.SaveChangesAsync();
            
            return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<Unit>.Failure(ErrorHelper.PrepareStorageException(EntityName));
        }
    }
}