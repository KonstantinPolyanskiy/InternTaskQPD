using Car.App.Models;
using Car.App.Models.CarModels;
using Car.App.Models.PhotoModels;
using Car.App.Repositories;
using Car.Dal.Models;

namespace Car.Dal.Repository.EntityFrameworkRepository;

public class PostgresPhotoRepository(AppDbContext dbContext) : IPhotoRepository
{
    private PhotoStorageType PhotoStorage => PhotoStorageType.Database;
    public async Task<PhotoResult> SavePhotoAsync(PhotoData request, CancellationToken ct = default)
    {
        byte[] bytes;
        await using (var ms = new MemoryStream())
        {
            await request.Content!.CopyToAsync(ms, ct);
            bytes = ms.ToArray();
        }

        var entity = new Photo(bytes, request.Extension);
        
        dbContext.Photos.Add(entity);
        await dbContext.SaveChangesAsync(ct);

        return new PhotoResult
        {
            Id = Convert.ToInt32(entity.Id),
            Extension = entity.Extension,
            PhotoStorage = PhotoStorage
        };
    }

    public async Task<PhotoResult?> GetPhotoAsync(string searchTerm)
    {
        if (int.TryParse(searchTerm, out var photoId) is false)
            throw new ArgumentException("Search term must be a number for database photo storage");

        var entity = await dbContext.Photos.FindAsync(photoId);
        if (entity is null)
            return null;

        return new PhotoResult
        {
            Id = Convert.ToInt32(entity.Id),
            Extension = entity.Extension,
            Bytes = entity.PhotoBytes,
            PhotoStorage = PhotoStorage
        };
    }

    public async Task<bool> DeletePhotoAsync(string searchTerm, CancellationToken ct = default)
    {
        if (int.TryParse(searchTerm, out var photoId) is false)
            throw new ArgumentException("Search term must be a number for database photo storage");   
        
        dbContext.Photos.Remove(new Photo
        {
            Id = photoId
        });
        
        await dbContext.SaveChangesAsync(ct);
        
        var photo = await GetPhotoAsync(searchTerm);
        
        return photo is null;
    }
}
