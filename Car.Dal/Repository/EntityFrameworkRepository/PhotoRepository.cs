using Car.Dal.Models;
using Contracts.Shared;

namespace Car.Dal.Repository.EntityFrameworkRepository;

public class PostgresPhotoRepository(AppDbContext dbContext) : IPhotoRepository
{
    public async Task<int> SavePhotoAsync(ICarPhoto photo, CancellationToken ct = default)
    {
        if (photo.Model?.Content is null) throw new ArgumentNullException(nameof(photo.Model.Content), "photo memory is null");
        
        await using var ms = new MemoryStream();
        await photo.Model.Content.CopyToAsync(ms, ct);

        var entity = new Photo(ms.ToArray(), photo.Model.FileExtension!);
        
        dbContext.Photos.Add(entity);
        await dbContext.SaveChangesAsync(ct);
    
        return entity.Id;
    }

    public async Task<ICarPhoto> GetPhotoAsync(int id, CancellationToken ct = default)
    {
        CarPhoto result = new CarPhoto();
        
        var entity = await dbContext.Photos.FindAsync([id], ct);
        if (entity?.PhotoBytes != null)
        {
            result.Id = entity.Id;
            result.Model = new ApplicationPhotoModel
            {
                Content = new MemoryStream(entity.PhotoBytes),
                FileExtension = entity.Extension,
                Length = entity.PhotoBytes.Length,
            };

            // считаем, что если имени - не хранится в бд
            result.PhotoName = null;
        }

        return result;
    }

    public async Task<bool> DeletePhotoAsync(int id, CancellationToken ct = default)
    {
        dbContext.Photos.Remove(new Photo(id));
        await dbContext.SaveChangesAsync(ct);
        
        var photo = await GetPhotoAsync(id, ct);

        return photo.Id is null;
    }
}