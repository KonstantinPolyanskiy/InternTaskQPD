using Car.Dal.Models;
using Contracts.Shared;

namespace Car.Dal.Repository.EntityFrameworkRepository;

public class PostgresPhotoRepository(AppDbContext dbContext) : IPhotoRepository
{
    public async Task<int> SavePhotoAsync(ApplicationPhotoModel photo, CancellationToken ct = default)
    {
        if (photo.Content is null) throw new ArgumentNullException(nameof(photo.Content), "photo memory is null");
        
        await using var ms = new MemoryStream();
        await photo.Content.CopyToAsync(ms, ct);

        var entity = new Photo
        {
            PhotoBytes = ms.ToArray(),
            FileName = photo.FileName,
            Extension = photo.FileExtension,
        };
        
        dbContext.Photos.Add(entity);
        await dbContext.SaveChangesAsync(ct);
    
        return entity.Id;
    }
        

    public async Task<ApplicationPhotoModel?> GetPhotoAsync(int id, CancellationToken ct = default)
    {
        var entity = await dbContext.Photos.FindAsync([id], ct);
        if (entity?.PhotoBytes is null) return null;

        return new ApplicationPhotoModel
        {
            Content = new MemoryStream(entity.PhotoBytes),
            FileName = entity.FileName,
            FileExtension = entity.Extension,
            Length = entity.PhotoBytes.Length,
        };
    }

    public async Task<bool> DeletePhotoAsync(int id, CancellationToken ct = default)
    {
        dbContext.Photos.Remove(new Photo { Id = id });
        await dbContext.SaveChangesAsync(ct);
        
        var photo = await GetPhotoAsync(id, ct);
        
        return photo is not null;
    }
}