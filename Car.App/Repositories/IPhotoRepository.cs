using Car.App.Models;
using Car.App.Models.PhotoModels;

namespace Car.App.Repositories;

public interface IPhotoRepository
{
    /// <summary> Сохраняет фото в хранилище </summary>
    public Task<PhotoResult> SavePhotoAsync(PhotoData data, CancellationToken ct = default);
    
    /// <summary>Получить фото по id </summary>
    /// <param name="searchTerm">Признак по которому ищется фото: id(int), batch+id, и тд</param>
    public Task<PhotoResult?> GetPhotoAsync(string searchTerm);
    
    /// <summary> Удаляет фото по id </summary>
    public Task<bool> DeletePhotoAsync(string searchTerm, CancellationToken ct = default);
}