using Backend.App.Models.Dto;
using Backend.App.Repositories;
using Backend.Dal.Models;
using Enum.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Dal.Repository.PostgresRepository;

public class PostgresPhotoRepository(AppDbContext dbContext, ILogger<PostgresPhotoRepository> log) : IPhotoRepository
{
    private PhotoStorageType PhotoStorageConst => PhotoStorageType.Database;
    public async Task<PhotoDto> SavePhotoAsync(PhotoDto dto, CancellationToken ct = default)
    {
        log.LogInformation("Сохранение фото в {@type}, таблица - {@table}", PhotoStorageConst, dbContext.Photos.EntityType.GetTableName());
        var entity = new PhotoEntity(dto);
        
        log.LogDebug("Данные для сохранения - {@data}", entity);
        
        dbContext.Photos.Add(entity);
        await dbContext.SaveChangesAsync(ct);
        
        log.LogInformation("В хранилище {@type} фото {@id} успешно сохранено фото, таблица - {@table}", PhotoStorageConst, entity.Id, dbContext.Photos.EntityType.GetTableName());

        return new PhotoDto
        {
            Id = entity.Id,
            Data = entity.PhotoBytes,
            StorageType = PhotoStorageConst
        };
    }

    public async Task<PhotoDto?> GetPhotoAsync(Guid photoId, PhotoStorageType storageType, CancellationToken ct = default)
    {
        log.LogInformation("Выгрузка фото с id {@id} из хранилища {@type}, таблица - {@table}", photoId, PhotoStorageConst, dbContext.Photos.EntityType.GetTableName());
        
        var entity = await dbContext.Photos
            .FirstOrDefaultAsync(p => p.Id == photoId, ct);
        if (entity is null) return null;

        log.LogInformation("С хранилища {@type} фото {@id} успешно получено, таблица - {@table}", PhotoStorageConst, entity.Id, dbContext.Photos.EntityType.GetTableName());
        return new PhotoDto
        {
            Id = entity.Id,
            Data = entity.PhotoBytes,
            StorageType = PhotoStorageConst
        };
    }

    public async Task DeletePhotoAsync(Guid photoId, PhotoStorageType storageType, CancellationToken ct = default)
    {
        log.LogInformation("Удаление фото с id {@id} из хранилища {@type}, таблица - {@table}", photoId, PhotoStorageConst, dbContext.Photos.EntityType.GetTableName());

        var entity = await dbContext.Photos.SingleOrDefaultAsync(p => p.Id == photoId, ct);
        if (entity is not null)
        {
            dbContext.Remove(entity);
            await dbContext.SaveChangesAsync(ct);
        }
        
        log.LogInformation("С хранилища {@type} фото {@id} успешно удалено, таблица - {@table}", PhotoStorageConst, photoId, dbContext.Photos.EntityType.GetTableName());
    }
}
