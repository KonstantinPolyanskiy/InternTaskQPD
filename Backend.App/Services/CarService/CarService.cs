using AutoMapper;
using Backend.App.Models;
using Backend.App.Models.Business;
using Backend.App.Models.Commands;
using Backend.App.Models.Dto;
using Backend.App.Repositories;
using Backend.App.Services.PhotoService;
using Enum.Common;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Backend.App.Services.CarService;

/// <summary>
/// Сервис для работы с сущностью машины
/// </summary>
public class CarService(ICarRepository carRepository, InternalPhotoService photoService,
    IMapper mapper, ILogger<CarService> log)
{
    /// <summary> Создать (добавить) машину </summary>
    public async Task<Car> CreateCarAsync(CreateCarCommand cmd)
    {
        log.LogInformation("Попытка добавления новой машины");
        
        var condition = !string.IsNullOrWhiteSpace(cmd.CurrentOwner) && cmd.Mileage!.Value > 0
            ? CarCondition.Used
            : CarCondition.New;
        if (cmd.Mileage is > 1000) condition = CarCondition.NotWorking;

        var carDto = new CarDto
        {
            Brand = cmd.Brand,
            Color = cmd.Color,
            Price = cmd.Price,
            CurrentOwner = cmd.CurrentOwner,
            Mileage = cmd.Mileage,
            Condition = condition,
            PrioritySale = condition switch
            {
                CarCondition.New => PrioritySale.High,
                CarCondition.Used => PrioritySale.Medium,
                CarCondition.NotWorking => PrioritySale.Low,
                CarCondition.Unknown or _ => PrioritySale.Unknown,
            },
        };
        
        log.LogDebug("Данные сохраняемой машины - {data}", carDto);

        var savedCar = await carRepository.CreateCarAsync(carDto);

        log.LogInformation("Машина с id {carId} успешно добавлена", savedCar.Id);
        
        return mapper.Map<Car>(savedCar);
    }
    
    /// <summary> Добавить к машине фото </summary>
    public async Task<Car> AddPhotoToCarAsync(SetPhotoToCarCommand cmd)
    {
        log.LogInformation("Попытка установить машине фотографию");
        
        // Проверяем есть ли вообще такая машина
        var car = await carRepository.GetCarByIdAsync(cmd.CarId);
        if (car == null) throw new ArgumentException($"Машина с Id - {cmd.CarId} не найдена");
        
        // Устанавливаем фото машине
        var savedPhoto = await photoService.SetPhotoToCarAsync(cmd);
        
        // Получаем новую версию машины
        var updatedCar = await carRepository.GetCarByIdAsync(cmd.CarId);
        if (updatedCar == null) 
            throw new ArgumentException($"Машина с Id - {cmd.CarId} не найдена");
        
        var carResult = mapper.Map<Car>(updatedCar);
        carResult.Photo = savedPhoto;
        
        log.LogInformation("Фото {photoId} успешно установлено машине {carId}", savedPhoto.Id, updatedCar.Id);
        
        return carResult;
    }

    /// <summary> Получить машину по ее id </summary>
    public async Task<Car> GetCarByIdAsync(SearchCarByIdCommand cmd)
    {
        log.LogInformation("Попытка найти машину по id");
        var carDto = await carRepository.GetCarByIdAsync(cmd.CarId);
        if (carDto is null) throw new ArgumentException($"Машина с Id - {cmd.CarId} не найдена");
        
        var car = mapper.Map<Car>(carDto);

        if (cmd.NeedPhoto && carDto.PhotoMetadataId is not null)
        {
            var photo = await photoService.GetPhotoByMetadataIdAsync(new SearchPhotoByMetadataIdCommand { MetadataId = (int)carDto.PhotoMetadataId });
            if (photo is not null) car.Photo = photo;
        }
        
        log.LogInformation("Машина с id - {carId} успешно найдена", car.Id);

        return car;
    }
    
    public sealed class CurrentUser
    {
        public string[] Roles { get; set; }
    }

    /// <summary> Обновить машину согласно переданным полям </summary>
    public async Task<Car> UpdateCarAsync(int id, CarData carData, CurrentUser user)
    {
        log.LogInformation("Попытка обновить машину");
        
        // Есть ли машина
        var iExists = await carRepository.Exists(id);
        if (iExists)
            throw new ArgumentException($"Машина с Id - {id} не найдена");
        
        log.LogDebug("Данные обновляемой машины - {carData}", carData);
        
        var updatedCar = await carRepository.UpdateCarAsync(id, carData);
        
        log.LogInformation("Машина с id - {carId} успешно обновлена", updatedCar.Id);

        if (user.Roles.Any(x => x == "Admin"))
        {
            var repositoryResponse = await carRepository.GetCarByIdAsync(id);
            // маппинг в carServiceResponse
            carServiceResponse.Manager = userRepository
        }
        else
        {
            var carRespomse = await carRepository.GetCarByIdAsync(id);
        }
        

        
        return car;
    }

    /// <summary> Удалить машину по id </summary>
    public async Task<bool> DeleteCarByIdAsync(DeleteCarCommand cmd)
    {
        log.LogInformation("Попытка удалить машину по id");
        
        if (cmd.HardDelete is false) throw new NotImplementedException("пока что не поддерживается");
        
        log.LogDebug("Данные удаляемой машины - {carData}", cmd);
        
        await carRepository.DeleteCarByIdAsync(cmd.Id);
        var car = await carRepository.GetCarByIdAsync(cmd.Id);

        if (car == null) log.LogInformation("Машина с id - {carId} успешно удалена", cmd.Id); 
        else log.LogWarning("Удалить машину с id - {carId} не получилось", cmd.Id);
        
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

    public async Task<CarPage> GetCarsByQueryAsync(SearchCarByQueryCommand cmd)
    {
        log.LogInformation("Попытка получить машины по параметрам");
        
        // Формируем запрос и приводим все к lowcase
        var query = mapper.Map<CarQueryDto>(cmd);
        query.Brands = cmd.Brands?.Select(b => b.ToLower()).ToArray();
        query.Colors = cmd.Colors?.Select(c => c.ToLower()).ToArray();
        
        log.LogDebug("Данные параметров - {@queryData}", query);
        
        var carPageDto = await carRepository.GetCarsByQueryAsync(mapper.Map<CarQueryDto>(cmd));
        
        var cars = carPageDto.Cars.Select(mapper.Map<Car>).ToList();
        var page = new CarPage
        {
            Cars = cars,
            PageNumber = carPageDto.PageNumber,
            PageSize = carPageDto.PageSize,
            TotalCount = carPageDto.TotalCount
        };
        
        log.LogInformation("Данные по параметрам успешно получены, всего - {allCount}, возврат - {currentCount}", page.TotalCount, cars.Count);

        if (cmd.PhotoTerm is PhotoHavingTerm.WithoutPhoto) return page;
        
        // Фото запрошены
        List<Car> carsWithPhoto = [];

        foreach (var cd in carPageDto.Cars)
        {
            if (cd.PhotoMetadataId.HasValue)
            {
                var photo = await photoService.GetPhotoByMetadataIdAsync(new SearchPhotoByMetadataIdCommand { MetadataId = (int)cd.PhotoMetadataId });
                if (photo is not null)
                {
                    var car = mapper.Map<Car>(cd);
                    car.Photo = photo;
                    carsWithPhoto.Add(car);
                }
            }
        }
        
        return new CarPage
        {
            Cars = carsWithPhoto.Count > 0 ? carsWithPhoto : cars,
            TotalCount = carPageDto.TotalCount,
            PageNumber = carPageDto.PageNumber,
            PageSize = carPageDto.PageSize
        };
    }
}

