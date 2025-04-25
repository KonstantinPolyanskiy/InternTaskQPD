using Car.App.Models;
using Car.App.Services.Repositories;
using Car.Dal.Models;

namespace Car.Dal.Repository.EntityFrameworkRepository;

public class PostgresPhotoRepository(AppDbContext dbContext) : IPhotoRepository
{
    private StorageType storage => StorageType.Database;
    public async Task<PhotoResult> SavePhotoAsync(PhotoData data, CancellationToken ct = default)
    {
        var entity = new Photo(data.Bytes, data.Extension);
        dbContext.Photos.Add(entity);
        await dbContext.SaveChangesAsync(ct);

        return new PhotoResult
        {
            Id = entity.Id,
            Extension = entity.Extension,
            Storage = storage
        };
    }

    public async Task<PhotoResult> GetPhotoAsync(string searchTerm, CancellationToken ct = default)
    {
        if (int.TryParse(searchTerm, out var photoId) is false)
            throw new ArgumentException("Search term must be a number for database photo storage");
     
        var entity = await dbContext.Photos.FindAsync([photoId], ct) ?? throw new ArgumentException("No photo found");

        return new PhotoResult
        {
            Id = entity.Id,
            Extension = entity.Extension,
            Storage = storage
        };
    }

    public async Task<bool> DeletePhotoAsync(string searchTerm, CancellationToken ct = default)
    {
        if (int.TryParse(searchTerm, out var photoId) is false)
            throw new ArgumentException("Search term must be a number for database photo storage");   
        
        dbContext.Photos.Remove(new Photo(photoId));
        await dbContext.SaveChangesAsync(ct);
        
        var photo = await GetPhotoAsync(photoId.ToString(), ct);

        return true;
    }
}
