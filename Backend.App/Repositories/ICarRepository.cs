using Backend.App.Models.Dto;

namespace Backend.App.Repositories;

public interface ICarRepository 
{
    /// <summary> Сохранить машину в хранилище </summary>
    public Task<CarDto> SaveCarAsync(CarDto dto);
    
    /// <summary> Получить машину по id </summary>
    public Task<CarDto?> GetCarByIdAsync(int id);
    
    /// <summary> Удалить машину по id </summary>
    public Task DeleteCarByIdAsync(int id);
    
    /// <summary> Получить все машины </summary>
    public Task<List<CarDto>> GetAllCarsAsync();

    /// <summary> Получить машины по параметрам и фильтрам </summary>
    public Task<CarPageDto> GetCarsByQueryAsync(CarQueryDto dto);
    
    /// <summary> Обновить машину </summary>
    public Task<CarDto?> UpdateCarAsync(CarDto dto);
}