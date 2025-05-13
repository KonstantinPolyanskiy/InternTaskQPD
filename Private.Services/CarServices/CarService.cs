using AutoMapper;
using Microsoft.Extensions.Logging;
using Private.Services.ErrorHelpers;
using Private.Services.Repositories;
using Private.ServicesInterfaces;
using Private.StorageModels;
using Public.Models.ApplicationErrors;
using Public.Models.BusinessModels.CarModels;
using Public.Models.BusinessModels.PhotoModels;
using Public.Models.BusinessModels.StorageModels;
using Public.Models.BusinessModels.UserModels;
using Public.Models.CommonModels;
using Public.Models.DtoModels.CarDtoModels;

namespace Private.Services.CarServices;

public class CarService : ICarService
{
    private readonly ILogger<CarService> _logger;
    
    private readonly ICarRepository _carRepository;

    private const string CarObjectName = "Car";

    // ReSharper disable once ConvertToPrimaryConstructor
    public CarService(ILogger<CarService> logger, ICarRepository carRepository)
    {
        _logger = logger;
        
        _carRepository = carRepository;
    }
    
    /// <summary>
    /// Создает и сохраняет машину без фото 
    /// </summary>
    /// <param name="carDto">Данные для создания машины</param>
    /// <returns><see cref="DomainCar"/> с Id <see cref="DomainEmployer"/> и без <see cref="DomainPhoto"/></returns>
    public async Task<ApplicationExecuteLogicResult<DomainCar>> CreateCarAsync(DtoForSaveCar carDto)
    {
        _logger.LogInformation("Попытка создать машину в системе");
        _logger.LogDebug("Данные для создания - {data}", carDto);
        
        var condition = CalculateCarCondition(carDto.CurrentOwner, carDto.Mileage);
        var priority = CalculatePrioritySale(condition);
        
        var carResult = await _carRepository.SaveCarAsync(new CarEntity
        {
            Brand = carDto.Brand,
            Color = carDto.Color,
            Price = carDto.Price,
            CurrentOwner = carDto.CurrentOwner,
            Mileage = carDto.Mileage,
            PrioritySale = priority,
            CarCondition = condition,
            ResponsiveManagerId = carDto.ResponsiveManager,
        });
        if (carResult.IsSuccess is not true)
        {
            if (carResult.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<CarEntity, DomainCar>(CarErrors.CarNotSaved, carResult);
        }
        var car = carResult.Value!;
        
        _logger.LogInformation("Машина с id {id} успешно сохранена", carResult.Value!.Id);
        
        return ApplicationExecuteLogicResult<DomainCar>
            .Success(new DomainCar
            {
                Id = car.Id,
                Brand = car.Brand,
                Color = car.Color,
                Price = car.Price,
                CurrentOwner = car.CurrentOwner,
                Mileage = car.Mileage,
                CarCondition = car.CarCondition,
                PrioritySale = car.PrioritySale,
                Manager = new DomainEmployer {Id = car.ResponsiveManagerId},
            });
    }
    
    /// <summary>
    /// Устанавливает машине метаданные фотографии
    /// </summary>
    /// <param name="car">Машина которой устанавливается фото</param>
    /// <param name="photo">Фото которое необходимо установить</param>
    /// <returns><see cref="DomainCar"/> с заполненным <see cref="DomainPhoto"/> </returns>
    public async Task<ApplicationExecuteLogicResult<DomainCar>> SetPhotoToCarAsync(DomainCar car, DomainPhoto photo)
    {
        _logger.LogInformation("Попытка установить машине фотографию");
        
        var oldCarResult = await _carRepository.GetCarByIdAsync(car.Id);
        if (oldCarResult.IsSuccess is not true)
        {
            if (oldCarResult.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<CarEntity, DomainCar>(CarErrors.CarNotUpdated, oldCarResult);
            if (oldCarResult.ContainsError(DatabaseErrors.NotFound))
                return ErrorHelper.WrapNotFoundError<CarEntity, DomainCar>(CarErrors.CarNotUpdated, CarObjectName, car.Id.ToString(), oldCarResult);
        }
        var oldCar = oldCarResult.Value!;
        
        oldCar.PhotoMetadataId = photo.Id;

        var updatedCarResult = await _carRepository.RewriteCarAsync(oldCar);
        if (oldCarResult.IsSuccess is not true)
        {
            if (oldCarResult.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<CarEntity, DomainCar>(CarErrors.CarNotUpdated, oldCarResult);
            if (oldCarResult.ContainsError(DatabaseErrors.NotFound))
                return ErrorHelper.WrapNotFoundError<CarEntity, DomainCar>(CarErrors.CarNotUpdated, CarObjectName, car.Id.ToString(), oldCarResult);
        }
        var updatedCar = updatedCarResult.Value!;
        
        _logger.LogInformation("Машине {carId} установлено фото с метаданными {metadataId} и данными {dataId}", updatedCar.Id, photo.Id, photo.PhotoDataId);
        
        return ApplicationExecuteLogicResult<DomainCar>
            .Success(new DomainCar
            {
                Id = updatedCar.Id,
                Brand = updatedCar.Brand,
                Color = updatedCar.Color,
                Price = updatedCar.Price,
                CurrentOwner = updatedCar.CurrentOwner,
                Mileage = updatedCar.Mileage,
                CarCondition = updatedCar.CarCondition,
                PrioritySale = updatedCar.PrioritySale,
                Manager = new DomainEmployer {Id = updatedCar.ResponsiveManagerId},
                Photo = new DomainPhoto
                {
                    Id = photo.Id,
                    CarId = updatedCar.Id,
                    Extension = photo.Extension,
                    PhotoDataId = photo.PhotoDataId,
                    PhotoData = photo.PhotoData
                }
            });
    }
    
    /// <summary>
    /// Обновляет стандартные данные машины
    /// </summary>
    /// <param name="carDto"> Данные для обновления </param>
    /// <returns><see cref="DomainCar"/> c Id (если есть, иначе - warn) <see cref="DomainPhoto"/> и Id <see cref="DomainEmployer"/></returns>
    public async Task<ApplicationExecuteLogicResult<DomainCar>> UpdateCarAsync(DtoForUpdateCar carDto)
    {
        _logger.LogInformation("Попытка обновить данные машины");
        _logger.LogDebug("Данные для обновления - {data}", carDto);
        
        // Получаем старую машину
        var oldCarResult = await _carRepository.GetCarByIdAsync(carDto.CarId);
        if (oldCarResult.IsSuccess is not true)
        {
            if (oldCarResult.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<CarEntity, DomainCar>(CarErrors.CarNotUpdated, oldCarResult);
            if (oldCarResult.ContainsError(DatabaseErrors.NotFound))
                return ErrorHelper.WrapNotFoundError<CarEntity, DomainCar>(CarErrors.CarNotUpdated, CarObjectName, carDto.CarId.ToString(), oldCarResult);
        }
        var oldCar = oldCarResult.Value!;
        
        // Переназначаем стандартные поля
        oldCar.Brand = carDto.Brand;
        oldCar.Color = carDto.Color;
        oldCar.Price = carDto.Price;
        
        if (carDto.NewManager is not null)
            oldCar.ResponsiveManagerId = (Guid)carDto.NewManager;
        
        var updatedCarResult = await _carRepository.RewriteCarAsync(oldCar);
        if (updatedCarResult.IsSuccess is not true)
        {
            if (updatedCarResult.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<CarEntity, DomainCar>(CarErrors.CarNotUpdated, updatedCarResult);
            if (updatedCarResult.ContainsError(DatabaseErrors.NotFound))
                return ErrorHelper.WrapNotFoundError<CarEntity, DomainCar>(CarErrors.CarNotUpdated, CarObjectName, carDto.CarId.ToString(), oldCarResult);
        }
        var updatedCar = oldCarResult.Value!;
        
        return ApplicationExecuteLogicResult<DomainCar>
            .Success(new DomainCar
            {
                Id = updatedCar.Id,
                Brand = updatedCar.Brand,
                Color = updatedCar.Color,
                Price = updatedCar.Price,
                CurrentOwner = updatedCar.CurrentOwner,
                Mileage = updatedCar.Mileage,
                CarCondition = updatedCar.CarCondition,
                PrioritySale = updatedCar.PrioritySale,
                Manager = new DomainEmployer {Id = updatedCar.ResponsiveManagerId},
                Photo = updatedCar.PhotoMetadataId is not null ? new DomainPhoto
                {
                    Id = (int)updatedCar.PhotoMetadataId,
                    CarId = updatedCar.Id,
                } : null,
            });
    }
    
    /// <summary>
    /// Возвращает машину
    /// </summary>
    /// <param name="id">Id машины</param>
    /// <returns><see cref="DomainCar"/> c Id (если есть, иначе - warn) <see cref="DomainPhoto"/> и Id <see cref="DomainEmployer"/></returns>
    public async Task<ApplicationExecuteLogicResult<DomainCar>> GetCarByIdAsync(int id)
    {
        _logger.LogInformation("Попытка найти машину с id {id}", id);
        
        var carResult = await _carRepository.GetCarByIdAsync(id);
        if (carResult.IsSuccess is not true)
        {
            if (carResult.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<CarEntity, DomainCar>(CarErrors.CarNotFound, carResult);
            if (carResult.ContainsError(DatabaseErrors.NotFound))
                return ErrorHelper.WrapNotFoundError<CarEntity, DomainCar>(CarErrors.CarNotFound, CarObjectName, id.ToString(), carResult);
        }
        var car = carResult.Value!;
        
        _logger.LogInformation("Машина с id {id} найдена", id);
        _logger.LogDebug("Данные найденной машины {@data}", car);

        var domainCar = new DomainCar
        {
            Id = car.Id,
            Brand = car.Brand,
            Color = car.Color,
            Price = car.Price,
            CurrentOwner = car.CurrentOwner,
            Mileage = car.Mileage,
            CarCondition = car.CarCondition,
            PrioritySale = car.PrioritySale,
            Manager = new DomainEmployer { Id = car.ResponsiveManagerId },
        };

        if (car.PhotoMetadataId is null)
        {
            return ApplicationExecuteLogicResult<DomainCar>.
                Success(domainCar)
                .WithWarning(new ApplicationError(
                    CarErrors.CarNotFoundPhoto, "Фото не найдено", 
                    $"У запрошенной машины отсутствует фото", ErrorSeverity.NotImportant));
        }

        domainCar.Photo = new DomainPhoto { Id = (int)car.PhotoMetadataId, CarId = car.Id };
        
        return ApplicationExecuteLogicResult<DomainCar>.Success(domainCar);
    }
    
    /// <summary>
    /// Hard удаление машины по id
    /// </summary>
    /// <param name="id">Id машины</param>
    /// <returns>Результат (с возможными ошибками) </returns>
    public async Task<ApplicationExecuteLogicResult<Unit>> DeleteCarByIdAsync(int id)
    {
        _logger.LogInformation("Удаление машины {id}", id);
        
        var deleteResult = await _carRepository.DeleteCarAsync(id);
        if (deleteResult.IsSuccess is not true)
        {
            if (deleteResult.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<Unit, Unit>(CarErrors.CarNotDeleted, deleteResult);
            if (deleteResult.ContainsError(DatabaseErrors.NotFound))
                return ErrorHelper.WrapNotFoundError<Unit, Unit>(CarErrors.CarNotFound, CarObjectName, id.ToString(), deleteResult);
        }
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }
    
    /// <summary>
    /// Возвращает машины по параметрам
    /// </summary>
    /// <param name="data">Параметры запроса и фильтры </param>
    /// <returns><see cref="DomainCar"/> c Id (если есть, иначе - warn) <see cref="DomainPhoto"/> и Id <see cref="DomainEmployer"/></returns>
    public async Task<ApplicationExecuteLogicResult<DomainCarsPage>> GetCarsAsync(DtoForSearchCars data)
    {
        _logger.LogInformation("Попытка найти машины по параметрам");
        _logger.LogDebug("Параметры - {@params}", data);

        var entityCarsResult = await _carRepository.GetCarsByQueryAsync(data);
        if (entityCarsResult.IsSuccess is not true)
        {
            if (entityCarsResult.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<CarsEntityPage, DomainCarsPage>(CarErrors.CarNotFound, entityCarsResult);
            if (entityCarsResult.ContainsError(DatabaseErrors.NotFound))
                return ErrorHelper.WrapNotFoundError<CarsEntityPage, DomainCarsPage>(CarErrors.CarNotFound, CarObjectName, "Query Search", entityCarsResult);
        }
        var entityCars = entityCarsResult.Value!;
        
        var domainCars = entityCars.Cars.Select(c => new DomainCar
        {
            Id = c.Id,
            Brand = c.Brand,
            Color = c.Color,
            Price = c.Price,
            CurrentOwner = c.CurrentOwner,
            Mileage = c.Mileage,
            CarCondition = c.CarCondition,
            PrioritySale = c.PrioritySale,
            Manager = new DomainEmployer { Id = c.ResponsiveManagerId },
            Photo = c.PhotoMetadataId is not null ? new DomainPhoto { Id = (int)c.PhotoMetadataId, CarId = c.Id} : null,
        }).ToList();
        
        _logger.LogInformation("Успешный поиск по параметрам, всего - {count}", entityCarsResult.Value!.Cars.Count);
        _logger.LogInformation("Результат domain cars - {@result}", domainCars);

        return ApplicationExecuteLogicResult<DomainCarsPage>.Success(new DomainCarsPage
        {
            DomainCars = domainCars,
            TotalCount = entityCarsResult.Value!.TotalCount,
            PageNumber = entityCarsResult.Value!.PageNumber,
            PageSize = entityCarsResult.Value!.PageSize,
        });
    }

    #region Вспомогательные методы

    private static CarConditionTypes CalculateCarCondition(string? owner, int? mileage)
    {
        if (string.IsNullOrWhiteSpace(owner) && mileage is null or <= 0)
            return CarConditionTypes.Now;
        
        if (mileage >= 10000)
            return CarConditionTypes.NotWork;
        
        return CarConditionTypes.Used;
    }

    private static PrioritySaleTypes CalculatePrioritySale(CarConditionTypes carCondition)
    {
        return carCondition switch
        {
            CarConditionTypes.Now => PrioritySaleTypes.High,
            CarConditionTypes.Used => PrioritySaleTypes.Medium,
            CarConditionTypes.NotWork => PrioritySaleTypes.Low,
            _ => PrioritySaleTypes.Unknown
        };
    }

    #endregion
}