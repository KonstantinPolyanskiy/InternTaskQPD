using Contracts.Shared;

namespace Car.Dal.Repository;

public interface IPhotoRepository
{
    /// <summary> Сохраняет фото в хранилище </summary>
    public Task<int> SavePhotoAsync(ICarPhoto photo, CancellationToken ct = default);
    
    /// <summary>Получить фото по id </summary>
    public Task<ICarPhoto> GetPhotoAsync(int id, CancellationToken ct = default);
    
    /// <summary> Удаляет фото по id </summary>
    public Task<bool> DeletePhotoAsync(int id, CancellationToken ct = default);
}