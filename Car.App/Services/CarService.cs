using AutoMapper;
using Car.App.Models.CarModels;
using Car.App.Models.Photo;
using Car.App.Models.PhotoModels;
using Car.App.Repositories;

namespace Car.App.Services;

/// <summary>
/// Сервис для работы с сущностью машины
/// </summary>
public class CarService(
    ICarRepository carRepository, IMapper mapper,
    IPhotoRepository photoRepository, PhotoProcessor photoProcessor
    )
{
    /// <summary> Создать (добавить) машину </summary>
    public async Task<int> CreateCarAsync(CarRequest carRequest)
    {
        var carData = mapper.Map<CarData>(carRequest);
        
        // Установка значений пока без логики
        carData.PrioritySale = CarPrioritySale.High;
        carData.Condition = string.IsNullOrWhiteSpace(carRequest.UsedCarDetail?.CurrentOwner) ? CarCondition.New : CarCondition.Used; 
        
        var savedCar = await carRepository.SaveCarAsync(carData);

        return savedCar.Id;
    }

    public async Task<CarPhoto> AddPhotoToCarAsync(PhotoRequest photoRequest)
    {
        var photoData = mapper.Map<PhotoData>(photoRequest);
        
        // TODO: в будущем менеджер должен указывать хранилище / иная логика
        photoData.PriorityPhotoStorage = PhotoStorageType.Database;
        photoData.UseOnlyPriority = true;
        
        var savedPhoto = await photoRepository.SavePhotoAsync(photoData);
        if (savedPhoto is null)
            throw new ApplicationException("cant save photo");

        var updatedCar = await carRepository.UpdateCarAsync(new CarData{ PhotoTermId = savedPhoto.Id.ToString(), StorageType = photoData.PriorityPhotoStorage}, photoData.CarId);
        if (updatedCar is null)
            throw new ApplicationException("cant find car to add photo");
        
        return GetCarPhoto(savedPhoto, PhotoMethod.Base64, updatedCar.StorageType);
    }

    /// <summary> Получить машину по ее id </summary>
    public async Task<Car.App.Models.CarModels.Car> GetCarByIdAsync(int carId)
    {
        var carResult = await carRepository.GetCarByIdAsync(carId);
        if (carResult is null)
            throw new ApplicationException("cant find car");


        var car = new Car.App.Models.CarModels.Car
        {
            Id = carResult.Id,
            Brand = carResult.Brand,
            Color = carResult.Color,
            Price = carResult.Price,
            CarCondition = (CarCondition)carResult.Condition!,
            PrioritySale = (CarPrioritySale)carResult.PrioritySale!,
        };

        if (carResult.PhotoTermId is not null)
        {
            var photoResult = await photoRepository.GetPhotoAsync(carResult.PhotoTermId);
            if (photoResult is not null)
            {
                photoResult.RequestedCarId = carResult.Id;
                car.Photo = GetCarPhoto(photoResult , PhotoMethod.Base64, carResult.StorageType);
            }
        }
        
        return car;
    }

    /// <summary> Обновить машину согласно переданным полям </summary>
    public async Task<Car.App.Models.CarModels.Car> UpdateCarAsync(CarRequest updatingCar, int carId)
    {
        var carData = mapper.Map<CarData>(updatingCar);
        var carResult = await carRepository.UpdateCarAsync(carData, carId);
        
        if (carResult is null)
            throw new ApplicationException("cant find car");


        var car = new Car.App.Models.CarModels.Car
        {
            Id = carResult.Id,
            Brand = carResult.Brand,
            Color = carResult.Color,
            Price = carResult.Price,
            CarCondition = (CarCondition)carResult.Condition!,
        };

        if (carResult.PhotoTermId is not null)
        {
            var photoResult = await photoRepository.GetPhotoAsync(carResult.PhotoTermId);
            if (photoResult is not null) 
                car.Photo = GetCarPhoto(photoResult , PhotoMethod.Base64, carResult.StorageType);
        }
        
        return car;
    }
    
    /// <summary> Удалить машину по id </summary>
    public async Task<bool> DeleteCarByIdAsync(int carId) => await carRepository.DeleteCarByIdAsync(carId);

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
            };

            if (!string.IsNullOrWhiteSpace(carResult.PhotoTermId))
            {
                var photoResult = await photoRepository.GetPhotoAsync(carResult.PhotoTermId);
                if (photoResult is not null)
                {
                    photoResult.RequestedCarId = carResult.Id;
                    car.Photo = GetCarPhoto(
                        photoResult,
                        PhotoMethod.Empty,
                        carResult.StorageType
                    );
                }
            }

            cars.Add(car);
        }

        return cars;
    }

    private CarPhoto GetCarPhoto(PhotoResult photoResult, PhotoMethod photoMethod, PhotoStorageType? photoStorageType) {
        // photo data
        var pd = new PhotoData
        {
            CarId = photoResult.RequestedCarId ?? throw new ApplicationException("cant find photo"),
            Extension = photoResult.Extension ?? string.Empty,
            PriorityPhotoStorage = photoStorageType,
        };
        
        // content
        if (photoResult.Bytes is not null)
            pd.Content = new MemoryStream(photoResult.Bytes ?? throw new InvalidOperationException("empty photo"));

        CarPhoto carPhoto = new CarPhoto
        {
            Id = photoResult.Id.ToString(),
            PhotoName = photoResult.Name,
            Data = pd,
        };
        
        // access
        var accessor = photoProcessor.ProcessPhoto(carPhoto, photoMethod, photoResult.RequestedCarId.ToString());

        carPhoto.Method = accessor.Method;
        carPhoto.Value = accessor.Value;

        return carPhoto;
    }
}

