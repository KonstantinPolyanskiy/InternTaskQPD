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
using Public.Models.CommonModels;
using Public.Models.DtoModels.CarDtoModels;

namespace Private.Services.CarServices;

public class CarService : ICarService
{
    private readonly ILogger<CarService> _logger;
    private readonly IMapper _mapper;
    
    private readonly ICarRepository _carRepository;

    private const string CarObjectName = "Car";

    // ReSharper disable once ConvertToPrimaryConstructor
    public CarService(ILogger<CarService> logger, IMapper mapper, ICarRepository carRepository)
    {
        _logger = logger;
        _mapper = mapper;
        
        _carRepository = carRepository;
    }
    
    public async Task<ApplicationExecuteLogicResult<DomainCar>> CreateCarAsync(DtoForCreateCar data)
    {
        _logger.LogInformation("Попытка создать машину в системе");
        _logger.LogDebug("Данные для создания - {data}", data);
        
        var condition = CalculateCarCondition(data.CurrentOwner, data.Mileage);
        var priority = CalculatePrioritySale(condition);
        
        var entity = _mapper.Map<CarEntity>(data);
        entity.CarCondition = condition;
        entity.PrioritySale = priority;
        
        var saved = await _carRepository.SaveCarAsync(entity);
        if (saved.IsSuccess is not true)
        {
            if (saved.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<CarEntity, DomainCar>(CarErrors.CarNotSaved, saved);
        }
        
        _logger.LogInformation("Машина с id {id} успешно сохранена", saved.Value!.Id);
        
        return ApplicationExecuteLogicResult<DomainCar>.Success(_mapper.Map<DomainCar>(saved.Value!));
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
        
        var car = _mapper.Map<DomainCar>(entity);
        car.Photo = new DomainPhoto
        {
            Id = (int)entity.Value!.PhotoMetadataId!,
            CarId = entity.Value!.Id,
        };
        
        _logger.LogInformation("Машина с id {id} найдена", id);
        _logger.LogDebug("Данные найденной машины {@data}", car);
        
        return ApplicationExecuteLogicResult<DomainCar>.Success(car);
    }

    public async Task<ApplicationExecuteLogicResult<DomainCarsPage>> GetCarsAsync(DtoForSearchCars data)
    {
        _logger.LogInformation("Попытка найти машины по параметрам");
        _logger.LogDebug("Параметры - {@params}", data);

        var entityCars = await _carRepository.GetCarsByQueryAsync(data);
        if (entityCars.IsSuccess is not true)
        {
            if (entityCars.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<CarsEntityPage, DomainCarsPage>(CarErrors.CarNotFound, entityCars);
            if (entityCars.ContainsError(DatabaseErrors.NotFound))
                return ErrorHelper.WrapNotFoundError<CarsEntityPage, DomainCarsPage>(CarErrors.CarNotFound, CarObjectName, "Query Search", entityCars);
        }
        
        var domainCars = entityCars.Value!.Cars
            .Select(car =>
            {
                var domainCar = _mapper.Map<DomainCar>(car);

                if (car.PhotoMetadataId is not null)
                {
                    domainCar.Photo = new DomainPhoto
                    {
                        Id           = car.PhotoMetadataId.Value,
                        CarId        = car.Id,
                        Extension    = ImageFileExtensions.Jpg,
                        PhotoDataId  = Guid.Empty,
                        PhotoData    = []
                    };
                }

                return domainCar;
            })
            .ToList();
        
        _logger.LogInformation("Успешный поиск по параметрам, всего - {count}", entityCars.Value!.Cars.Count);
        _logger.LogInformation("Результат domain cars - {@result}", domainCars);

        return ApplicationExecuteLogicResult<DomainCarsPage>.Success(new DomainCarsPage
        {
            DomainCars = domainCars,
            TotalCount = entityCars.Value!.TotalCount,
            PageNumber = entityCars.Value!.PageNumber,
            PageSize = entityCars.Value!.PageSize,
        });
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