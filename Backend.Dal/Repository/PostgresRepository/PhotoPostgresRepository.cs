using Backend.App.Models.Dto;
using Backend.App.Repositories;
using Backend.Dal.Models;
using Enum.Common;
using Microsoft.EntityFrameworkCore;

namespace Backend.Dal.Repository.PostgresRepository;

public class PostgresPhotoRepository(AppDbContext dbContext) : IPhotoRepository
{
    private PhotoStorageType PhotoStorageConst => PhotoStorageType.Database;
    public async Task<PhotoDto> SavePhotoAsync(PhotoDto dto, CancellationToken ct = default)
    {
        var entity = new PhotoEntity(dto);
        
        dbContext.Photos.Add(entity);
        await dbContext.SaveChangesAsync(ct);

        return new PhotoDto
        {
            Id = entity.Id,
            Data = entity.PhotoBytes,
            StorageType = PhotoStorageConst
        };
    }

    public async Task<PhotoDto?> GetPhotoAsync(Guid photoId, PhotoStorageType storageType, CancellationToken ct = default)
    {
        var entity = await dbContext.Photos
            .FirstOrDefaultAsync(p => p.Id == photoId, ct);
        if (entity is null) return null;

        return new PhotoDto
        {
            Id = entity.Id,
            Data = entity.PhotoBytes,
            StorageType = PhotoStorageConst
        };
    }

    public async Task DeletePhotoAsync(Guid photoId, PhotoStorageType storageType, CancellationToken ct = default)
    {
        var entity = await dbContext.Photos.SingleOrDefaultAsync(p => p.Id == photoId, ct);
        if (entity is not null)
        {
            dbContext.Remove(entity);
            await dbContext.SaveChangesAsync(ct);
        }
    }
}
