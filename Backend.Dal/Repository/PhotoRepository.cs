using Backend.App.Models.Dto;
using Backend.App.Repositories;
using Backend.Dal.Repository.MinioRepository;
using Backend.Dal.Repository.PostgresRepository;
using Enum.Common;

namespace Backend.Dal.Repository;

public class PhotoRepository(PostgresPhotoRepository postgresRepository, PhotoMinioRepository minioRepository) : IPhotoRepository
{
    public async Task<PhotoDto> SavePhotoAsync(PhotoDto data, CancellationToken ct = default)
    {
        var priority = data.StorageType ?? PhotoStorageType.Minio;

        try
        {
            return await ResolveRepository(priority).SavePhotoAsync(data, ct);
        }
        catch (Exception) when ((bool)!data.UseOnlyPriorityStorage!)
        {
        }
        
        var secondary = GetSecondary(priority);
        return await ResolveRepository(secondary).SavePhotoAsync(data, ct);
    }

    public async Task<PhotoDto?> GetPhotoAsync(Guid id, PhotoStorageType storageType, CancellationToken ct = default)
    {
        return await ResolveRepository(storageType).GetPhotoAsync(id, storageType, ct);
    }

    public async Task DeletePhotoAsync(Guid id, PhotoStorageType storageType, CancellationToken ct = default)
    {
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