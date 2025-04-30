using AutoMapper;
using Car.App.Models.CarModels;
using Car.App.Models.Dto;
using Car.App.Models.Photo;
using Car.App.Models.PhotoModels;
using Car.App.Repositories;

namespace Car.App.Services.CarService;

/// <summary>
/// Сервис для работы с сущностью машины
/// </summary>
public class CarService(
    ICarRepository carRepository, IMapper mapper,
    IPhotoRepository photoRepository, PhotoProcessor photoProcessor
    )
{
    // TODO: в будущем заменить на настройки/в зав-ти от запроса
    private PhotoStorageType PhotoStorageConst => PhotoStorageType.Database;
    private bool UseOnlyPriorityConst => true;
    
    /// <summary> Создать (добавить) машину </summary>
    public async Task<int> CreateCarAsync(CarRequestDataDto carRequestDataDto)
    {
        var carData = mapper.Map<CarDto>(carRequestDataDto);
        
        // Установка значений пока без логики
        carData.PrioritySale = CarPrioritySale.High;
        carData.Condition = string.IsNullOrWhiteSpace(carRequestDataDto.UsedCarDetail?.CurrentOwner) ? CarCondition.New : CarCondition.Used; 
        
        var savedCar = await carRepository.SaveCarAsync(carData);

        return savedCar.Id;
    }

    public async Task<CarPhoto> AddPhotoToCarAsync(PhotoRequestDto photoRequest)
    {
        // Проверяем есть ли вообще такая машина
        var car = carRepository.GetCarByIdAsync(photoRequest.CarId);
        if (car == null)
            throw new ArgumentException($"Машина с Id - {photoRequest.CarId} не найдена");
            
        // Подготавливаем данные для хранилища
        var photoDataToSave = CarServiceHelper.PreparePhotoDataDto(photoRequest, PhotoStorageConst, UseOnlyPriorityConst);
        
        // Сохраняем
        var savedPhoto = await photoRepository.SavePhotoAsync(photoDataToSave);
        if (savedPhoto is null)
            throw new ApplicationException("не получилось сохранить фото");
        
        // Обновляем машину с сохранным фото
        var updatedCar = await carRepository.UpdateCarAsync(new CarDto
            { 
                PhotoTermId = savedPhoto.TermId, 
                StorageType = savedPhoto.PhotoStorage,
            },
            car.Id);
        if (updatedCar is null)
            throw new ArgumentException($"Машина с Id - {car.Id} не найдена");
        
        return CarServiceHelper.PrepareCarPhoto(savedPhoto, photoProcessor, PhotoMethod.Base64);
    }

    /// <summary> Получить машину по ее id </summary>
    public async Task<Car.App.Models.CarModels.Car> GetCarByIdAsync(int carId)
    {
        CarPhoto? carPhoto = null;
        
        // Ищем машину
        var carResult = await carRepository.GetCarByIdAsync(carId);
        if (carResult is null)
            throw new ArgumentException($"Машина с Id - {carId} не найдена");
        
        // Ищем фото
        if (carResult.PhotoTermId is not null)
            carPhoto = CarServiceHelper.PrepareCarPhoto(
                await photoRepository.GetPhotoAsync(carResult.PhotoTermId),
                photoProcessor, PhotoMethod.Base64); // используем пока base64 формат

        return new Car.App.Models.CarModels.Car
        {
            Id = carResult.Id,
            
            Brand = carResult.Brand,
            Color = carResult.Color,
            Price = carResult.Price,
            
            CarCondition = carResult.Condition!.Value,
            PrioritySale = carResult.PrioritySale!.Value,
            
            Photo = carPhoto
        };
    }

    /// <summary> Обновить машину согласно переданным полям </summary>
    public async Task<Car.App.Models.CarModels.Car> UpdateCarAsync(CarRequestDataDto updatingCar, int carId)
    {
        CarPhoto? carPhoto = null;
        
        // Есть ли машина
        var car = carRepository.GetCarByIdAsync(carId);
        if (car is null)
            throw new ArgumentException($"Машина с Id - {carId} не найдена");
        
        // Подготавливаем и обновляем данные
        var carData = mapper.Map<CarDto>(updatingCar);
        var updatedCar = await carRepository.UpdateCarAsync(carData, car.Id);
        if (updatedCar is null)
            throw new ArgumentException($"Машина с Id - {car.Id} не найдена");
        
        // Ищем фото
        if (updatedCar.PhotoTermId is not null)
            carPhoto = CarServiceHelper.PrepareCarPhoto(
                await photoRepository.GetPhotoAsync(updatedCar.PhotoTermId),
                photoProcessor, PhotoMethod.Empty); // используем пока base64 формат
        
        return new Car.App.Models.CarModels.Car
        {
            Id = updatedCar.Id,
            
            Brand = updatedCar.Brand,
            Color = updatedCar.Color,
            Price = updatedCar.Price,
            
            CarCondition = updatedCar.Condition!.Value,
            PrioritySale = updatedCar.PrioritySale!.Value,
            
            Photo = carPhoto
        };
    }

    /// <summary> Удалить машину по id </summary>
    public async Task<bool> DeleteCarByIdAsync(int carId)
    {
        await carRepository.DeleteCarByIdAsync(carId);
        var car = await carRepository.GetCarByIdAsync(carId);
        return car is null;
    }

    /// <summary> Получить все машины </summary>
    public async Task<IList<Car.App.Models.CarModels.Car>> GetCarsAsync()
    {
        var carResults = await carRepository.GetAllCarsAsync();

        var cars = new List<Car.App.Models.CarModels.Car>(carResults.Count);
        foreach (var carResult in carResults)
        {
            var car = new Car.App.Models.CarModels.Car
            {
                Id            = carResult.Id,
                Brand         = carResult.Brand,
                Color         = carResult.Color,
                Price         = carResult.Price,
                
                CarCondition  = (CarCondition)carResult.Condition!,
                PrioritySale  = (CarPrioritySale)carResult.PrioritySale!,
                
                Photo = carResult.PhotoTermId is not null 
                    ? CarServiceHelper.PrepareCarPhoto(await photoRepository.GetPhotoAsync(carResult.PhotoTermId),
                    photoProcessor, PhotoMethod.Empty) 
                    
                    : null
            };
            cars.Add(car);
        }

        return cars;
    }
}

