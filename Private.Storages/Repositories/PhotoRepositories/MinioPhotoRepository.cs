using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Private.Services.Repositories;
using Private.StorageModels;
using Private.Storages.ErrorHelpers;
using Public.Models.BusinessModels.StorageModels;
using Public.Models.CommonModels;

namespace Private.Storages.Repositories.PhotoRepositories;

public class MinioPhotoRepository(IMinioClient minioClient, ILogger<MinioPhotoRepository> logger) : IPhotoRepository
{
    private const string PhotoBucket = "PhotoBucket";
    
    public async Task<ApplicationExecuteLogicResult<PhotoEntity>> SavePhotoAsync(PhotoEntity entity, StorageTypes priority, ImageFileExtensions? ext = null)
    {
        try
        {
            await CheckAndCreateBucket(PhotoBucket);
            
            var photoId = Guid.NewGuid();
            
            using var ms = new MemoryStream(entity.PhotoBytes);

            var args = new PutObjectArgs()
                .WithBucket(PhotoBucket)
                .WithObject(photoId.ToString())
                .WithStreamData(ms)
                .WithObjectSize(ms.Length)
                .WithContentType("image/" + ext);

            await minioClient.PutObjectAsync(args);

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
            await CheckAndCreateBucket(PhotoBucket);

            await using var ms = new MemoryStream();

            var args = new GetObjectArgs()
                .WithBucket(PhotoBucket)
                .WithObject(id.ToString())
                .WithCallbackStream(async st => { await st.CopyToAsync(ms); });

            await minioClient.GetObjectAsync(args);

            return ApplicationExecuteLogicResult<PhotoEntity>.Success(new PhotoEntity
            {
                Id = id,
                PhotoBytes = ms.ToArray()
            });
        }
        catch (ObjectNotFoundException)
        {
            return ApplicationExecuteLogicResult<PhotoEntity>.Failure(
                ErrorHelper.PrepareNotFoundError(PhotoRepository.EntityName));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<PhotoEntity>.Failure(
                ErrorHelper.PrepareStorageException(PhotoRepository.EntityName));
        }
    }

    public Task<ApplicationExecuteLogicResult<List<PhotoEntity>>> GetPhotosByIdsAsync(Guid[] ids)
    {
        throw new NotImplementedException();
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> DeletePhotoByIdAsync(Guid id, StorageTypes storageType)
    {
        try
        {
            var args = new RemoveObjectArgs()
                .WithBucket(PhotoBucket)
                .WithObject(id.ToString());

            await minioClient.RemoveObjectAsync(args);
            
            return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
        }
        catch (DeleteObjectException)
        {
            return ApplicationExecuteLogicResult<Unit>.Failure(
                ErrorHelper.PrepareNotDeletedError(PhotoRepository.EntityName));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ApplicationExecuteLogicResult<Unit>.Failure(
                ErrorHelper.PrepareStorageException(PhotoRepository.EntityName));
        }
    }

    private async Task CheckAndCreateBucket(string bucketName)
    {
        var exist = await minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
        if (exist is false)
            await minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
    }
}


