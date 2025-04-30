using Car.App.Models.Dto;

namespace Car.App.Repositories;

public interface ICarRepository 
{
    /// <summary>
    /// Сохранить машину в хранилище
    /// </summary>
    public Task<CarResultDto> SaveCarAsync(CarDto dto);
    
    /// <summary>
    /// Получить машину по id
    /// </summary>
    public Task<CarResultDto?> GetCarByIdAsync(int id);
    
    /// <summary>
    /// Удалить машину по id
    /// </summary>
    /// <returns>true если удалена</returns>
    public Task DeleteCarByIdAsync(int id);
    
    /// <summary>
    /// Получить все машины
    /// </summary>
    /// <returns>Все машины из таблицы </returns>
    public Task<List<CarResultDto>> GetAllCarsAsync();
    
    /// <summary>
    /// Обновить машину по id
    /// </summary>
    /// <param name="dto">Поля для обновления (должны быть не null для обновления)</param>
    /// <param name="id">id обновляемой машины</param>
    /// <returns>Обновленная машина, null - если такой нет</returns>
    public Task<CarResultDto?> UpdateCarAsync(CarDto dto, int id);
}