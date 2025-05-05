using Backend.App.Models.Dto;
using Enum.Common;

namespace Backend.App.Repositories;

public interface IPhotoRepository
{
    /// <summary> Сохраняет фото в хранилище </summary>
    public Task<PhotoDto> SavePhotoAsync(PhotoDto data, CancellationToken ct = default);
    
    /// <summary>Получить фото по id </summary>
    public Task<PhotoDto?> GetPhotoAsync(Guid id, PhotoStorageType storageType, CancellationToken ct = default);
    
    /// <summary> Удаляет фото по id </summary>
    public Task DeletePhotoAsync(Guid id, PhotoStorageType storageType, CancellationToken ct = default);
}