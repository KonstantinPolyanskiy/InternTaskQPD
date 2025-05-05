using AutoMapper;
using Backend.App.Models.Business;
using Backend.App.Models.Commands;
using Backend.App.Models.Dto;
using Backend.App.Repositories;
using Backend.App.Services.PhotoService;
using Enum.Common;

namespace Backend.App.Services.CarService;

/// <summary>
/// Сервис для работы с сущностью машины
/// </summary>
public class CarService(IMapper mapper,
    ICarRepository carRepository,
    InternalPhotoService photoService)
{
    /// <summary> Создать (добавить) машину </summary>
    public async Task<Car> CreateCarAsync(CreateCarCommand cmd)
    {
        var condition = string.IsNullOrWhiteSpace(cmd.CurrentOwner) || cmd.Mileage > 0
            ? CarCondition.Used
            : CarCondition.New;

        var carDto = new CarDto
        {
            Brand = cmd.Brand,
            Color = cmd.Color,
            Price = cmd.Price,
            CurrentOwner = cmd.CurrentOwner,
            Mileage = cmd.Mileage,
            Condition = condition,
            PrioritySale = condition == CarCondition.Used ? PrioritySale.High : PrioritySale.Medium,
        };

        var savedCar = await carRepository.SaveCarAsync(carDto);

        return mapper.Map<Car>(savedCar);
    }

    public async Task<Car> AddPhotoToCarAsync(SetPhotoToCarCommand cmd)
    {
        // Проверяем есть ли вообще такая машина
        var car = await carRepository.GetCarByIdAsync(cmd.CarId);
        if (car == null) throw new ArgumentException($"Машина с Id - {cmd.CarId} не найдена");
        
        // Устанавливаем фото машине
        var savedPhoto = await photoService.SetPhotoToCarAsync(cmd);
        
        // Получаем новую версию машины
        var updatedCar = await carRepository.GetCarByIdAsync(cmd.CarId);
        var carResult = mapper.Map<Car>(updatedCar);
        carResult.Photo = savedPhoto;
        
        return carResult;
    }

    /// <summary> Получить машину по ее id </summary>
    public async Task<Car> GetCarByIdAsync(SearchCarCommand cmd)
    {
        var carDto = await carRepository.GetCarByIdAsync(cmd.CarId);
        if (carDto is null) throw new ArgumentException($"Машина с Id - {cmd.CarId} не найдена");
        
        var car = mapper.Map<Car>(carDto);

        if (cmd.NeedPhoto && carDto.PhotoMetadataId is not null)
        {
            var photo = await photoService.GetPhotoByMetadataIdAsync(new SearchPhotoByMetadataIdCommand { MetadataId = (int)carDto.PhotoMetadataId });
            if (photo is not null) car.Photo = photo;
        }

        return car;
    }

    /// <summary> Обновить машину согласно переданным полям </summary>
    public async Task<Car> UpdateCarAsync(UpdateCarCommand cmd)
    {
        // Есть ли машина
        var carEntity = carRepository.GetCarByIdAsync(cmd.Id);
        if (carEntity is null) throw new ArgumentException($"Машина с Id - {cmd.Id} не найдена");
        
        // Подготавливаем и обновляем данные
        var carData = mapper.Map<CarDto>(cmd);
        
        var updatedCar = await carRepository.UpdateCarAsync(carData);
        if (updatedCar is null) throw new ArgumentException($"Машина с Id - {carEntity.Id} не найдена");
        
        var car = mapper.Map<Car>(updatedCar);
        
        return car;
    }

    /// <summary> Удалить машину по id </summary>
    public async Task<bool> DeleteCarByIdAsync(DeleteCarCommand cmd)
    {
        if (cmd.HardDelete is false) throw new NotImplementedException("пока что не поддерживается");
        
        await carRepository.DeleteCarByIdAsync(cmd.Id);
        var car = await carRepository.GetCarByIdAsync(cmd.Id);
        return car is null;
    }

    /// <summary> Получить все машины </summary>
    public async Task<IList<Car>> GetCarsAsync()
    {
        var carResults = await carRepository.GetAllCarsAsync();

        var cars = new List<Car>(carResults.Count);
        foreach (var carResult in carResults)
            cars.Add(mapper.Map<Car>(carResult));

        return cars;
    }
}

