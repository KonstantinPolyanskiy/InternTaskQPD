using AutoMapper;
using Car.App.Models;
using Car.Dal.Repository;
using Contracts.Dtos;
using Contracts.Shared;
using Contracts.Types;

namespace Car.App.Services;

/// <summary>
/// Сервис для работы с сущностью машины
/// </summary>
public class CarService(ICarRepository carRepository, IPhotoRepository photoRepository, IMapper mapper)
{
    /// <summary>
    /// Создать (добавить) машину в системе
    /// </summary>
    /// <param name="carServicesDto">Данные для добавления</param>
    /// <returns>Созданная машина</returns>
    public async Task<ICar> CreateCarAsync(AddedCarServicesDto carServicesDto)
    {
        var carType = DetectCarType(carServicesDto);

        // Создаем DTO для data layer с вычисленным типом
        var dataDto = mapper.Map<AddedCarDataLayerDto>(
            carServicesDto,
            opts => opts.Items["CarType"] = carType);

        var photoId = await SavePhotoAsync(dataDto.Photo, photoRepository);

        var carId = await carRepository.SaveCarAsync(dataDto, photoId);
        
        // Получаем сохраненную машину и возвращаем
        return mapper.Map<ICar>(await GetCarByIdAsync(carId));
    }

    /// <summary>
    /// Получить машину по ее id
    /// </summary>
    /// <param name="carId">id машины</param>
    /// <returns>Машина с переданным id</returns>
    /// <exception cref="ApplicationException">Не получилось найти машину с переданным id</exception>
    public async Task<ICar> GetCarByIdAsync(int carId)
    {
        var carEntity = await carRepository.GetCarByIdAsync(carId);
        if (carEntity is null) throw new ApplicationException("Car could not be found");
        
        var car = mapper.Map<ICar>(carEntity);
        if (car is null) throw new ApplicationException("fail convert entity to business object ");
        
        return car;
    }

    public async Task<ICar> UpdateCarAsync(PatchCarServicesDto carServicesDto, int id)
    {
        var dataDto = mapper.Map<UpdatedCarDataLayerDto>(carServicesDto);
        
        var updatedCarEntity = await carRepository.UpdateCarAsync(dataDto, id);
        if (updatedCarEntity is null) throw new ApplicationException("Failed to update car");
        
        return mapper.Map<ICar>(updatedCarEntity);
    }
    
    /// <summary>
    /// Удалить машину по ее id
    /// </summary>
    /// <param name="carId">id машины</param>
    /// <returns>Удалена ли машина</returns>
    public async Task<bool> DeleteCarByIdAsync(int carId)
    {
        return await carRepository.DeleteCarByIdAsync(carId);
    }

    /// <summary>
    /// Возвращает все машины
    /// </summary>
    public async Task<IList<ICar>> GetCarsAsync()
    {
        List<ICar> cars = new List<ICar>();
        
        foreach (var entity in await carRepository.GetAllCarsAsync())
        {
            cars.Add(mapper.Map<ICar>(entity));
        }
        
        return cars;
    }

    /// <summary>
    /// Определяет тип машины
    /// </summary>
    /// <param name="carServicesDto">Набор признаков для определения типа машины</param>
    /// <returns>Тип машины</returns>
    private static CarTypes DetectCarType(AddedCarServicesDto carServicesDto)
    {
        // Есть владелец и пробег больше 0 - БУ
        if (!string.IsNullOrWhiteSpace(carServicesDto.CurrentOwner) && carServicesDto.Mileage <= 0) return CarTypes.UsedCar;
        
        // Во всех остальных случаях - новая машина
        return CarTypes.NewCar;
    }
    
    /// <summary>
    /// Сохраняет саму фотографию в хранилище
    /// </summary>
    /// <param name="photo">Фото</param>
    /// <param name="photoRepository">Репозиторий для сохранения фото</param>
    /// <returns>id сохраненного фото, null - если сохранить не получилось</returns>
    private static async Task<int?> SavePhotoAsync(ApplicationPhotoModel? photo, IPhotoRepository photoRepository)
    {
        int? photoId = null;

        if (photo is not null)
        {
            photoId = await photoRepository.SavePhotoAsync(photo);
        }
        
        return photoId;
    } 
}