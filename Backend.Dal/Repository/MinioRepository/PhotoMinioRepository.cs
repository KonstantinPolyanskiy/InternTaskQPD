using Backend.App.Models.Dto;
using Backend.App.Repositories;
using Enum.Common;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Serilog;

namespace Backend.Dal.Repository.MinioRepository;

public class PhotoMinioRepository(IMinioClient minioClient, ILogger<PhotoMinioRepository> log) : IPhotoRepository
{
    private const string PhotoBucket = "minio";
    private PhotoStorageType PhotoStorageConst => PhotoStorageType.Minio;
    
    public async Task<PhotoDto> SavePhotoAsync(PhotoDto data, CancellationToken ct = default)
    {
        log.LogInformation("Сохранение фото в {@type}, bucket - {@bucket}", PhotoStorageConst, PhotoBucket);
        
        var exist = await minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(PhotoBucket), ct);
        if (!exist)
            await minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(PhotoBucket), ct);

        var id = Guid.NewGuid();
        
        using var ms = new MemoryStream(data.Data!);

        var args = new PutObjectArgs()
            .WithBucket(PhotoBucket)
            .WithObject(id.ToString())
            .WithStreamData(ms)
            .WithObjectSize(ms.Length)
            .WithContentType("image/" + data.Extension);
        
        log.LogDebug("Данные для сохранения - {@data}", args);

        try
        {
            await minioClient.PutObjectAsync(args, ct);
        }
        catch (MinioException ex)
        {
            log.LogError(ex, "Не удалось загрузить фото в {@type}, bucket - {@bucket}", PhotoStorageConst, PhotoBucket);
            throw;
        }
        
        log.LogInformation("В хранилище {@type} фото {@id} успешно сохранено фото. bucket - {@bucket}", PhotoStorageConst, id, PhotoBucket);
        
        return new PhotoDto
        {
            Id = id,
            StorageType = PhotoStorageConst,
            Data = ms.ToArray(),
            Extension = data.Extension
        };
    }

    public async Task<PhotoDto?> GetPhotoAsync(Guid id, PhotoStorageType storageType, CancellationToken ct = default)
    {
        try
        {
            log.LogInformation("Выгрузка фото с id {@id} из хранилища {@type}, bucket - {@bucket}", id,
                PhotoStorageConst, PhotoBucket);
            var stat = await minioClient.StatObjectAsync(
                new StatObjectArgs()
                    .WithBucket(PhotoBucket)
                    .WithObject(id.ToString()),
                ct);

            var ext = stat.ContentType.Split('/').TakeLast(1).ToString();

            await using var ms = new MemoryStream();

            var args = new GetObjectArgs()
                .WithBucket(PhotoBucket)
                .WithObject(id.ToString())
                .WithCallbackStream(async stream => { await stream.CopyToAsync(ms, ct); });

            log.LogDebug("Данные для поиска - {@data}", args);

            await minioClient.GetObjectAsync(args, ct);

            log.LogInformation("С хранилища {@type} фото {@id} успешно получено. bucket - {@bucket}", PhotoStorageConst,
                id, PhotoBucket);
            
            return new PhotoDto
            {
                Id = id,
                Extension = PhotoFileExtensionHelper.MapExtension(ext!),
                Data = ms.ToArray(),
                StorageType = PhotoStorageConst
            };
        }
        catch (ObjectNotFoundException)
        {
            log.LogInformation("В хранилище {@type} фото {@id} не было найдено. bucket - {@bucket}", PhotoStorageConst, id, PhotoBucket);
            return null!;
        }
        catch (MinioException ex)
        {
            log.LogError(ex, "Не удалось получить фото {@id} в {@type}, bucket - {@bucket}", id, PhotoStorageConst, PhotoBucket);
            throw;
        }
    }

    public async Task DeletePhotoAsync(Guid id, PhotoStorageType storageType, CancellationToken ct = default)
    {
        log.LogInformation("Удаление фото с id {@id} из хранилища {@type}, bucket - {@bucket}", id, PhotoStorageConst, PhotoBucket);
        
        var args = new RemoveObjectArgs()
            .WithBucket(PhotoBucket)
            .WithObject(id.ToString());
        log.LogDebug("Данные для удаления - {@data}", args);

        try
        {
            await minioClient.RemoveObjectAsync(args, ct);

        }
        catch (DeleteObjectException ex)
        {
            log.LogError(ex, "Не удалось удалить фото {@id} в {@type}, bucket - {@bucket}", id, PhotoStorageConst, PhotoBucket);
            throw;
        }
        
        log.LogInformation("С хранилища {@type} фото {@id} успешно удалено. bucket - {@bucket}", PhotoStorageConst, id, PhotoBucket);
    }
}