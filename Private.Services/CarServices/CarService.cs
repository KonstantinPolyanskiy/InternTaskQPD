using Microsoft.Extensions.Logging;
using Private.Services.ErrorHelpers;
using Private.Services.Repositories;
using Private.ServicesInterfaces;
using Private.StorageModels;
using Public.Models.ApplicationErrors;
using Public.Models.BusinessModels.CarModels;
using Public.Models.BusinessModels.PhotoModels;
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
    
    public async Task<ApplicationExecuteLogicResult<DomainCar>> CreateCarAsync(DtoForCreateCar data)
    {
        _logger.LogInformation("Попытка создать машину в системе");
        _logger.LogDebug("Данные для создания - {data}", data);
        
        var condition = CalculateCarCondition(data.CurrentOwner, data.Mileage);
        var priority = CalculatePrioritySale(condition);
        
        // TODO: добавить mapper
        var entity = new CarEntity
        {
            Brand = data.Brand,
            Color = data.Color,
            Price = data.Price,
            
            CurrentOwner = data.CurrentOwner,
            Mileage = data.Mileage,
            
            PrioritySale = priority,
            CarCondition = condition,
        };
        
        var saved = await _carRepository.SaveCarAsync(entity);
        if (saved.IsSuccess is not true)
        {
            if (saved.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<CarEntity, DomainCar>(CarErrors.CarNotSaved, saved);
        }
        
        _logger.LogInformation("Машина с id {id} успешно сохранена", saved.Value!.Id);

        // TODO: добавить mapper
        var car = new DomainCar
        {
            Id = saved.Value!.Id,
            Brand = saved.Value!.Brand,
            Color = saved.Value!.Color,
            Price = saved.Value!.Price,
            CurrentOwner = saved.Value!.CurrentOwner,
            Mileage = saved.Value!.Mileage,
        };
        
        return ApplicationExecuteLogicResult<DomainCar>.Success(car);
    }

    public async Task<ApplicationExecuteLogicResult<DomainCar>> GetCarAsyncById(int id)
    {
        _logger.LogInformation("Попытка найти машину с id {id}", id);
        
        var entity = await _carRepository.GetCarByIdAsync(id);
        if (entity.IsSuccess is not true)
        {
            if (entity.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<CarEntity, DomainCar>(CarErrors.CarNotFound, entity);
            if (entity.ContainsError(DatabaseErrors.NotFound))
                return ErrorHelper.WrapNotFoundError<CarEntity, DomainCar>(CarErrors.CarNotFound, CarObjectName, id.ToString(), entity);
        }
        
        // TODO: добавить automapper
        var car = new DomainCar
        {
            Id = entity.Value!.Id,
            Brand = entity.Value!.Brand,
            Color = entity.Value!.Color,
            Price = entity.Value!.Price,
            CurrentOwner = entity.Value!.CurrentOwner,
            Mileage = entity.Value!.Mileage,
            PrioritySale = entity.Value!.PrioritySale,
            CarCondition = entity.Value!.CarCondition,
            Photo = new DomainPhoto
            {
                CarId = entity.Value!.Id,
                Id = (int)entity.Value!.PhotoMetadataId!
            }
        };
        
        _logger.LogInformation("Машина с id {id} найдена", id);
        _logger.LogDebug("Данные найденной машины {@data}", car);
        
        return ApplicationExecuteLogicResult<DomainCar>.Success(car);
    }

    public async Task<ApplicationExecuteLogicResult<List<DomainCar>>> GetCarsAsync(DtoForSearchCars data)
    {
        var entityCars = _carRepository.
    }

    public async Task<ApplicationExecuteLogicResult<DomainCar>> SetPhotoMetadataToCarAsync(DomainCar car, int photoMetadataId)
    {
        _logger.LogInformation("Попытка назначить машине фото");
        _logger.LogDebug("Id метаданных - {id}", photoMetadataId);
        
        var entity = await _carRepository.GetCarByIdAsync(car.Id);
        if (entity.IsSuccess is not true)
        {
            if (entity.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<CarEntity, DomainCar>(CarErrors.CarNotUpdated, entity);
            if (entity.ContainsError(DatabaseErrors.NotFound))
                return ErrorHelper.WrapNotFoundError<CarEntity, DomainCar>(CarErrors.CarNotUpdated, CarObjectName, car.Id.ToString(), entity);
        }
        
        entity.Value!.PhotoMetadataId = photoMetadataId;
        
        var updated = await _carRepository.RewriteCarAsync(entity.Value!);
        if (updated.IsSuccess is not true)
        {
            if (updated.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<CarEntity, DomainCar>(CarErrors.CarNotUpdated, updated);
            if (updated.ContainsError(DatabaseErrors.NotFound))
                return ErrorHelper.WrapNotFoundError<CarEntity, DomainCar>(CarErrors.CarNotUpdated, CarObjectName, car.Id.ToString(), updated);
        }
        
        return ApplicationExecuteLogicResult<DomainCar>.Success(car);
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