using Car.App.Models;
using Car.App.Models.CarModels;

namespace Car.App.Repositories;

public interface ICarRepository 
{
    /// <summary>
    /// Сохранить машину в хранилище
    /// </summary>
    public Task<CarResult> SaveCarAsync(CarData data);
    
    /// <summary>
    /// Получить машину по id
    /// </summary>
    public Task<CarResult?> GetCarByIdAsync(int id);
    
    /// <summary>
    /// Удалить машину по id
    /// </summary>
    /// <returns>true если удалена</returns>
    public Task<bool> DeleteCarByIdAsync(int id);
    
    /// <summary>
    /// Получить все машины
    /// </summary>
    /// <returns>Все машины из таблицы </returns>
    public Task<List<CarResult>> GetAllCarsAsync();
    
    /// <summary>
    /// Обновить машину по id
    /// </summary>
    /// <param name="dto">Поля для обновления (должны быть не null для обновления)</param>
    /// <param name="id">id обновляемой машины</param>
    /// <returns>Обновленная машина, null - если такой нет</returns>
    public Task<CarResult?> UpdateCarAsync(CarData dto, int id);
}