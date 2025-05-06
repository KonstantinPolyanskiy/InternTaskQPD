using Backend.App.Models.Dto;
using Backend.App.Repositories;
using Backend.Dal.Repository.MinioRepository;
using Backend.Dal.Repository.PostgresRepository;
using Enum.Common;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Backend.Dal.Repository;

public class PhotoRepository(ILogger<PhotoRepository> log,
    PostgresPhotoRepository postgresRepository, PhotoMinioRepository minioRepository) : IPhotoRepository
{
    public async Task<PhotoDto> SavePhotoAsync(PhotoDto data, CancellationToken ct = default)
    {
        var priority = data.StorageType ?? PhotoStorageType.Minio;
        
        log.LogInformation("Сохранение фото в хранилище, тип хранилища - {@storage}, использовать только приоритетное - {usePrio}", priority, (bool)data.UseOnlyPriorityStorage! ? "да" : "нет");
        
        try
        {
            Log.Information("Попытка сохранить фото в репозиторий - {@repo}", ResolveRepository(priority).GetType());
            var photo = await ResolveRepository(priority).SavePhotoAsync(data, ct);
            if (photo == null) throw new ApplicationException("Не получилось сохранить фото");
            
            log.LogInformation("В репозиторий {@repo} c типом {@type} успешно сохранено фото {photoId}", ResolveRepository(priority).GetType(), priority, photo.Id);
            return photo;
        }
        catch (Exception) when ((bool)!data.UseOnlyPriorityStorage!)
        {
        }
        
        var secondary = GetSecondary(priority);
        
        log.LogWarning("Сохранить фото в приоритетное хранилище {type} не получилось, попытка сохранить в {secondary}", priority, secondary);
        
        return await ResolveRepository(secondary).SavePhotoAsync(data, ct);
    }

    public async Task<PhotoDto?> GetPhotoAsync(Guid id, PhotoStorageType storageType, CancellationToken ct = default)
    {
        log.LogInformation("Получение фото {id} из хранилища {type}", id, storageType);
        return await ResolveRepository(storageType).GetPhotoAsync(id, storageType, ct);
    }

    public async Task DeletePhotoAsync(Guid id, PhotoStorageType storageType, CancellationToken ct = default)
    {
        log.LogInformation("Удаление фото {id} из хранилища {type}", id, storageType);
        await ResolveRepository(storageType).DeletePhotoAsync(id, storageType, ct);
    }
    
    private IPhotoRepository ResolveRepository(PhotoStorageType type) =>
        type switch
        {
            PhotoStorageType.Minio    => minioRepository,
            PhotoStorageType.Database => postgresRepository,
            _                         => throw new ArgumentOutOfRangeException(nameof(type))
        };
    
    private static PhotoStorageType GetSecondary(PhotoStorageType priority) =>
        priority == PhotoStorageType.Minio
            ? PhotoStorageType.Database
            : PhotoStorageType.Minio;
}