using Backend.App.Models.Dto;
using Backend.App.Repositories;
using Backend.Dal.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Dal.Repository.PostgresRepository;

public class PhotoMetadataPostgresRepository(AppDbContext dbContext) : IPhotoMetadataRepository
{
    public async Task<PhotoMetadataDto> SavePhotoMetadataAsync(PhotoMetadataDto dto)
    {
        var entity = new PhotoMetadataEntity(dto);
        
        dbContext.PhotosMetadata.Add(entity);
        await dbContext.SaveChangesAsync();

        return new PhotoMetadataDto
        {
            Id = entity.Id,
            PhotoId = entity.PhotoId,
            Extension = entity.Extension,
            StorageType = entity.StorageType,
            CarId = entity.CarId,
        };
    }

    public async Task<PhotoMetadataDto?> GetPhotoMetadataAsync(int metadataId)
    {
        var entity = await dbContext.PhotosMetadata.FirstOrDefaultAsync(m => m.Id == metadataId);
        if (entity is null) return null;

        return new PhotoMetadataDto
        {
            Id = entity.Id,
            PhotoId = entity.PhotoId,
            Extension = entity.Extension,
            StorageType = entity.StorageType,
            CarId = entity.CarId,
        };
    }
}