using Car.App.Models.CarModels;
using Car.App.Models.Dto;
using Car.App.Models.PhotoModels;
using Car.App.Repositories;
using Car.Dal.Models;
using Microsoft.EntityFrameworkCore;

namespace Car.Dal.Repository.PostgresRepository;

public class PostgresPhotoRepository(AppDbContext dbContext) : IPhotoRepository
{
    private PhotoStorageType PhotoStorageConst => PhotoStorageType.Database;
    public async Task<PhotoResultDto> SavePhotoAsync(PhotoDataDto dto, CancellationToken ct = default)
    {
        var entity = new PhotoEntity(dto);
        
        dbContext.Photos.Add(entity);
        await dbContext.SaveChangesAsync(ct);

        return new PhotoResultDto
        {
            TermId = entity.Id.ToString(),
            CarId = entity.Car?.Id,
            
            Bytes = entity.PhotoBytes,
            Name = string.Empty,
            Extension = entity.Extension,
            
            // Назначаем тип хранилища, у каждой реализации IPhotoRepository он свой
            PhotoStorage = PhotoStorageConst
        };
    }

    public async Task<PhotoResultDto?> GetPhotoAsync(string photoTermId)
    {
        if (int.TryParse(photoTermId, out var photoId) is false)
            throw new ArgumentException("Для данного хранилища поддерживается только целочисленный Id");

        var entity = await dbContext.Photos
            .Include(p => p.Car)
            .FirstOrDefaultAsync(p => p.Id == photoId);
        if (entity is null)
            return null;

        return new PhotoResultDto
        {
            TermId = entity.Id.ToString(),
            CarId = entity.Car?.Id,
            
            Extension = entity.Extension,
            Bytes = entity.PhotoBytes,
            Name = string.Empty,
            
            PhotoStorage = PhotoStorageConst
        };
    }

    public async Task DeletePhotoAsync(string photoTermId, CancellationToken ct = default)
    {
        if (int.TryParse(photoTermId, out var photoId) is false)
            throw new ArgumentException("Для данного хранилища поддерживается только целочисленный Id");

        var entity = await dbContext.Photos.SingleOrDefaultAsync(p => p.Id == photoId, ct);
        if (entity is not null)
        {
            dbContext.Remove(entity);
            await dbContext.SaveChangesAsync(ct);
        }
    }
}
