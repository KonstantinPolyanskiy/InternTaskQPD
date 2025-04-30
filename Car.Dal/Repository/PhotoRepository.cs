using Car.App.Models.CarModels;
using Car.App.Models.Dto;
using Car.App.Repositories;
using Car.Dal.Repository.MinioRepository;
using Car.Dal.Repository.PostgresRepository;

namespace Car.Dal.Repository;

public class PhotoRepository(PostgresPhotoRepository postgresRepository, PhotoMinioRepository minioRepository) : IPhotoRepository
{
    public async Task<PhotoResultDto> SavePhotoAsync(PhotoDataDto data, CancellationToken ct = default)
    {
        var priority = data.PriorityPhotoStorage ?? PhotoStorageType.Minio;

        try
        {
            return await ResolveRepository(priority).SavePhotoAsync(data, ct);
        }
        catch (Exception) when (!data.UseOnlyPriorityStorage)
        {
        }
        
        var secondary = GetSecondary(priority);
        return await ResolveRepository(secondary).SavePhotoAsync(data, ct);
    }

    public Task<PhotoResultDto?> GetPhotoAsync(string searchTerm)
    {
        throw new NotImplementedException();
    }

    public Task DeletePhotoAsync(string searchTerm, CancellationToken ct = default)
    {
        throw new NotImplementedException();
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