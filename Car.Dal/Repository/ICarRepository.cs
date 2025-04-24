using Contracts.Dtos;

namespace Car.Dal.Repository;

public interface ICarRepository 
{
    /// <summary>
    /// Сохранить машину в хранилище
    /// </summary>
    public Task<int> SaveCarAsync(AddedCarDataLayerDto dto, int? photoId = null);
    
    /// <summary>
    /// Получить машину по id
    /// </summary>
    public Task<Models.Car?> GetCarByIdAsync(int id);
    
    /// <summary>
    /// Удалить машину по id
    /// </summary>
    /// <returns>true если удалена</returns>
    public Task<bool> DeleteCarByIdAsync(int id);
    
    /// <summary>
    /// Получить все машины
    /// </summary>
    /// <returns>Все машины из таблицы </returns>
    public Task<List<Models.Car>> GetAllCarsAsync();
    
    /// <summary>
    /// Обновить машину по id
    /// </summary>
    /// <param name="newCar">Поля для обновления (должны быть не null)</param>
    /// <param name="id">id обновляемой машины</param>
    /// <returns>Обновленная машина, null - если такой нет</returns>
    public Task<Models.Car?> UpdateCarAsync(Models.Car newCar, int id);
}