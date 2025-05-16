using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Private.Services.Repositories;
using Private.StorageModels;
using Private.Storages.DbContexts;
using Private.Storages.ErrorHelpers;
using Public.Models.BusinessModels.StorageModels;
using Public.Models.CommonModels;

namespace Private.Storages.Repositories.PhotoRepositories;

public class PhotoPostgresRepository(AppDbContext db, ILogger<PhotoPostgresRepository> logger) : IPhotoRepository
{
    public async Task<ApplicationExecuteLogicResult<PhotoEntity>> SavePhotoAsync(PhotoEntity entity, StorageTypes priority, ImageFileExtensions? extensions = null)
    {
        try
        {
            await db.Photos.AddAsync(entity);
            await db.SaveChangesAsync();
            
            return ApplicationExecuteLogicResult<PhotoEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<PhotoEntity>.Failure(
                ErrorHelper.PrepareStorageException(PhotoRepository.EntityName));
        }
    }

    public async Task<ApplicationExecuteLogicResult<PhotoEntity>> GetPhotoByIdAsync(Guid id)
    {
        try
        {
            var entity = await db.Photos.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return ApplicationExecuteLogicResult<PhotoEntity>.Failure(
                    ErrorHelper.PrepareNotFoundError(PhotoRepository.EntityName));
            
            return ApplicationExecuteLogicResult<PhotoEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<PhotoEntity>.Failure(
                ErrorHelper.PrepareStorageException(PhotoRepository.EntityName));
        }
    }

    public async Task<ApplicationExecuteLogicResult<List<PhotoEntity>>> GetPhotosByIdsAsync(Guid[] ids)
    {
        try
        {
            var entities = await db.Photos.Where(x => ids.Contains(x.Id)).ToListAsync();
            if (entities.Count == 0)
                return ApplicationExecuteLogicResult<List<PhotoEntity>>.Failure(
                    ErrorHelper.PrepareNotFoundError(PhotoRepository.EntityName));
            
            return ApplicationExecuteLogicResult<List<PhotoEntity>>.Success(entities);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<List<PhotoEntity>>.Failure(
                ErrorHelper.PrepareStorageException(PhotoRepository.EntityName));
        }
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> DeletePhotoByIdAsync(Guid id, StorageTypes storageType)
    {
        try
        {
            var entity = await db.Photos.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return ApplicationExecuteLogicResult<Unit>.Failure(
                    ErrorHelper.PrepareNotFoundError(PhotoRepository.EntityName));
            
            db.Photos.Remove(entity);
            await db.SaveChangesAsync();
            
            return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<Unit>.Failure(
                ErrorHelper.PrepareStorageException(PhotoRepository.EntityName));
        }
    }
}