using Backend.App.Models;
using Backend.App.Models.Dto;

namespace Backend.App.Repositories;

public interface ICarRepository 
{
    /// <summary> Сохранить машину в хранилище </summary>
    public Task<CarDto> CreateCarAsync(CarDto dto);
    
    /// <summary> Получить машину по id </summary>
    public Task<CarDto?> GetCarByIdAsync(int id);    
    
    /// <summary>Существует ли автомобиль</summary>
    public Task<bool> Exists(int id);
    
    /// <summary> Удалить машину по id </summary>
    public Task DeleteCarByIdAsync(int id);
    
    /// <summary> Получить все машины </summary>
    public Task<List<CarDto>> GetAllCarsAsync();

    /// <summary> Получить машины по параметрам и фильтрам </summary>
    public Task<CarPageDto> GetCarsByQueryAsync(CarQueryDto dto);
    
    /// <summary> Обновить машину </summary>
    public Task UpdateCarAsync(int id, CarData dto);
    
    /// <summary> Сбросить у машины метаданные фотографии </summary>
    public Task DeleteMetadataAsync(int id);
}