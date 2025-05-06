using Backend.App.Models.Dto;

namespace Backend.App.Repositories;

public interface IPhotoMetadataRepository
{
    public Task<PhotoMetadataDto> SavePhotoMetadataAsync(PhotoMetadataDto dto);
    public Task<PhotoMetadataDto?> GetPhotoMetadataAsync(int metadataId);
    public Task DeleteMetadataAsync(int metadataId);
}