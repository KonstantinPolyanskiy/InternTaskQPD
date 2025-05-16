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
using Public.Models.DtoModels.PhotoDtoModels;
using Public.Models.Extensions;

namespace Private.Services.CarServices;

public class CarService(ICarRepository carRepository, IPhotoRepository photoRepository,
    IPhotoMetadataRepository photoMetadataRepository, IEmployerService employerService, ILogger<CarService> logger) : ICarService
{
    private const string CarObjectName = "Car";
    private const string PhotoObjectName = "Photo";
    private const string PhotoMetadataObjectName = "Photo Metadata";

    public async Task<ApplicationExecuteLogicResult<DomainCar>> CreateCarAsync(DtoForSaveCar carDto)
    {
        logger.LogInformation("Попытка создать машину в системе");
        
        var condition = CalculateCarCondition(carDto.CurrentOwner, carDto.Mileage);
        var priority = CalculatePrioritySale(condition);
        
        var carResult = await carRepository.SaveCarAsync(new CarEntity
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
            return ErrorHelper.WrapAllDbErrors<CarEntity, DomainCar>(CarErrors.CarNotSaved, carResult, string.Join(" ", CarObjectName));
        var carEntity = carResult.Value!;
        
        logger.LogInformation("Машина с id {id} успешно сохранена", carEntity.Id);
        
        var car = new DomainCar
        {
            Id = carEntity.Id,
            Brand = carEntity.Brand,
            Color = carEntity.Color,
            Price = carEntity.Price,
            CurrentOwner = carEntity.CurrentOwner,
            Mileage = carEntity.Mileage,
            CarCondition = carEntity.CarCondition,
            PrioritySale = carEntity.PrioritySale,
            Manager = new DomainEmployer
            {
                Id = carDto.ResponsiveManager,
            },
        };
        
        var warns = new List<ApplicationError>();

        if (carDto.Photo is not null)
        {
            carDto.Photo.CarId = carEntity.Id;
            var savedPhotoResult = await SetCarPhoto(carDto.Photo);
            if (savedPhotoResult.IsSuccess is false)
                warns.Add(new ApplicationError(
                    CarErrors.CarAddedWithoutPhoto, "Не удалось сохранить фото",
                    $"Для машины {carEntity.Id} не получилось сохранить фото",
                    ErrorSeverity.NotImportant));
            
            car = savedPhotoResult.Value!;
        }
                
        var managerResult = await employerService.ManagerByUserIdAsync(carDto.ResponsiveManager);
        if (managerResult.IsSuccess is false)
            warns.Add(new ApplicationError(
                    UserErrors.UserNotFound, "Менеджер машины не найден",
                    $"Не получилось найти менеджера машины {car.Id}",
                    ErrorSeverity.NotImportant));
        var manager = managerResult.Value!;
        
        car.Manager = manager;
        
        if (car.Photo is null)
            warns.Add(new ApplicationError(
                CarErrors.CarNotFoundPhoto, "Фото машины не найдено",
                $"У машины {car.Id} не установлено фото",
                ErrorSeverity.NotImportant));

        return ApplicationExecuteLogicResult<DomainCar>.Success(car).WithWarnings(warns);
    }
    public async Task<ApplicationExecuteLogicResult<DomainCar>> SetCarPhoto(DtoForSavePhoto photoDto)
    {
        logger.LogInformation("Попытка установить фото машине {carId}", photoDto.CarId);

        var imageResult = await photoRepository.SavePhotoAsync(new PhotoEntity { PhotoBytes = photoDto.PhotoData }, StorageTypes.Minio, photoDto.Extension);
        if (imageResult.IsSuccess is false)
            return ErrorHelper.WrapAllDbErrors<PhotoEntity, DomainCar>(PhotoImageErrors.ImageNotSaved, imageResult, PhotoObjectName);
        var photoEntity = imageResult.Value!;
        
        var metadataResult = await photoMetadataRepository.SavePhotoMetadataAsync(new PhotoMetadataEntity
        {
            PhotoDataId = photoEntity.Id,
            StorageType = photoDto.PriorityStorageType,
            Extension = photoDto.Extension,
            CarId = photoDto.CarId,
        });
        if (metadataResult.IsSuccess is false)
            return ErrorHelper.WrapAllDbErrors<PhotoMetadataEntity, DomainCar>(PhotoMetadataErrors.MetadataNotSaved, metadataResult, PhotoMetadataObjectName);
        var metadataEntity = metadataResult.Value!;
        
        var carResult = await carRepository.GetCarByIdAsync(metadataEntity.CarId);
        if (carResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<DomainCar>.Failure().Merge(carResult);
        var withoutCarEntity = carResult.Value!;
        
        withoutCarEntity.PhotoMetadataId = metadataEntity.Id;

        var updateCarResult = await carRepository.RewriteCarAsync(withoutCarEntity);
        if (updateCarResult.IsSuccess is false)
            return ErrorHelper.WrapAllDbErrors<CarEntity, DomainCar>(CarErrors.CarNotUpdated, updateCarResult, string.Join(" ", CarObjectName, withoutCarEntity.Id.ToString()));
        var withPhotoCarEntity = updateCarResult.Value!;

        var updatedCarResult = await CarById(withoutCarEntity.Id);
        if (updatedCarResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<DomainCar>.Failure().Merge(updatedCarResult);
        var car = updatedCarResult.Value!;
        
        
        return ApplicationExecuteLogicResult<DomainCar>.Success(car);
    }
    public async Task<ApplicationExecuteLogicResult<DomainCar>> CarById(int id)
    {
        logger.LogInformation("Получение машины {carId}", id);
        
        var carResult = await carRepository.GetCarByIdAsync(id);
        if (carResult.IsSuccess is false)
            return ErrorHelper.WrapAllDbErrors<CarEntity, DomainCar>(CarErrors.CarNotFound, carResult, string.Join(" ", CarObjectName, id.ToString()));
        var carEntity = carResult.Value!;

        var managerResult = await employerService.ManagerByUserIdAsync(carEntity.ResponsiveManagerId);
        if (managerResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<DomainCar>.Failure().Merge(managerResult);
        var manager = managerResult.Value!;

        var car = new DomainCar
        {
            Id = carEntity.Id,
            Brand = carEntity.Brand,
            Color = carEntity.Color,
            Price = carEntity.Price,
            CurrentOwner = carEntity.CurrentOwner,
            Mileage = carEntity.Mileage,
            CarCondition = carEntity.CarCondition,
            PrioritySale = carEntity.PrioritySale,
            Manager = manager,
        };
        
        if (carEntity.PhotoMetadataId is null)
            return ApplicationExecuteLogicResult<DomainCar>.Success(car);


        var photoResult = await CarPhotoById((int)carEntity.PhotoMetadataId);
        if (photoResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<DomainCar>.Success(car).WithWarning(new ApplicationError(
                CarErrors.CarNotFoundPhoto, "Нет фото", "У запрашиваемой машины отсутствует фото",
                ErrorSeverity.NotImportant));
        var photo = photoResult.Value!;

        car.Photo = photo;
        
        return ApplicationExecuteLogicResult<DomainCar>.Success(car);
    }
    public async Task<ApplicationExecuteLogicResult<DomainCarsPage>> CarsByParams(DtoForSearchCars searchDto)
    {
        logger.LogInformation("Попытка найти машины по параметрам {@data}", searchDto);
        
        var entityCarsResult = await carRepository.GetCarsByQueryAsync(searchDto);
        if (entityCarsResult.IsSuccess is false)
                return ErrorHelper.WrapAllDbErrors<CarsEntityPage, DomainCarsPage>(CarErrors.CarNotFound, entityCarsResult, string.Join(" ", CarObjectName+"s", searchDto.ToString()));
        var entityCars = entityCarsResult.Value!;
        
        var warns = new List<ApplicationError>();
        var cars = new List<DomainCar>();

        foreach (var entity in entityCars.Cars)
        {
            var carResult = await CarById(entity.Id);
            warns.AddRange(carResult.GetWarnings);
            
            cars.Add(carResult.Value!);
        }

        return ApplicationExecuteLogicResult<DomainCarsPage>.Success(new DomainCarsPage
        {
            DomainCars = cars,
            TotalCount = entityCars.TotalCount,
            PageNumber = entityCars.PageNumber,
            PageSize = entityCars.PageSize,
        }).WithWarnings(warns);
    }
    public async Task<ApplicationExecuteLogicResult<DomainCar>> UpdateCarAsync(DtoForUpdateCar carDto)
    {
        logger.LogInformation("Попытка обновить данные машины {carId}", carDto.CarId);
        
        // Получаем старую машину
        var oldCarResult = await carRepository.GetCarByIdAsync(carDto.CarId);
        if (oldCarResult.IsSuccess is false)
            return ErrorHelper.WrapAllDbErrors<CarEntity, DomainCar>(CarErrors.CarNotUpdated, oldCarResult, string.Join(" ", CarObjectName, carDto.CarId.ToString()));
        var oldCar = oldCarResult.Value!;
        
        // Переназначаем стандартные поля
        oldCar.Brand = carDto.Brand;
        oldCar.Color = carDto.Color;
        oldCar.Price = carDto.Price;
        
        if (carDto.NewManager is not null)
            oldCar.ResponsiveManagerId = (Guid)carDto.NewManager;
        
        var updatedCarResult = await carRepository.RewriteCarAsync(oldCar);
        if (updatedCarResult.IsSuccess is not true)
            return ErrorHelper.WrapAllDbErrors<CarEntity, DomainCar>(CarErrors.CarNotUpdated, updatedCarResult, string.Join(" ", CarObjectName, carDto.CarId.ToString()));
        var updatedCar = oldCarResult.Value!;
        
        var warns = new List<ApplicationError>();
        
        var managerResult = await employerService.ManagerByUserIdAsync(updatedCar.ResponsiveManagerId);
        if (managerResult.IsSuccess is false)
            warns.Add(new ApplicationError(
                UserErrors.UserNotFound, "Менеджер машины не найден",
                $"Не получилось найти менеджера машины {updatedCar.Id}",
                ErrorSeverity.NotImportant));
        var manager = managerResult.Value!;
        
        var photoResult = await CarPhotoById((int)updatedCar.PhotoMetadataId!);
        if (photoResult.IsSuccess is false)
            warns.Add(new ApplicationError(
                CarErrors.CarNotFoundPhoto, "Фото машины не найдено",
                $"У машины {updatedCar.Id} не получилось найти фото",
                ErrorSeverity.NotImportant));
        var photo = photoResult.Value!;
        
        
        return ApplicationExecuteLogicResult<DomainCar>.Success(new DomainCar
            {
                Id = updatedCar.Id,
                Brand = updatedCar.Brand,
                Color = updatedCar.Color,
                Price = updatedCar.Price,
                CurrentOwner = updatedCar.CurrentOwner,
                Mileage = updatedCar.Mileage,
                CarCondition = updatedCar.CarCondition,
                PrioritySale = updatedCar.PrioritySale,
                Manager = manager,
                Photo = photo,
            }).WithWarnings(warns);
    }
    public async Task<ApplicationExecuteLogicResult<Unit>> DeleteCarById(int id)
    {
        logger.LogInformation("Удаление машины {carId}", id);
        
        var deleteResult = await carRepository.DeleteCarAsync(id);
        if (deleteResult.IsSuccess is false)
            return ErrorHelper.WrapAllDbErrors<Unit, Unit>(CarErrors.CarNotDeleted, deleteResult, string.Join(" ", CarObjectName, id.ToString()));
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }
    public async Task<ApplicationExecuteLogicResult<DomainPhoto>> CarPhotoById(int metadataId)
    {
        logger.LogInformation("Получение фото по метаданным {metadataId}", metadataId);
        
        var metadataResult = await photoMetadataRepository.GetPhotoMetadataByIdAsync(metadataId);
        if (metadataResult.IsSuccess is false)
            return ErrorHelper.WrapAllDbErrors<PhotoMetadataEntity, DomainPhoto>(PhotoMetadataErrors.MetadataNotFound, metadataResult, string.Join(" ", PhotoMetadataObjectName, metadataId.ToString()));
        var metadata = metadataResult.Value!;

        var photoResult = await photoRepository.GetPhotoByIdAsync((Guid)metadata.PhotoDataId!);
        if (photoResult.IsSuccess is false)
            return ErrorHelper.WrapAllDbErrors<PhotoEntity, DomainPhoto>(PhotoImageErrors.ImageNotFound, photoResult, string.Join(" ", PhotoObjectName, metadata.PhotoDataId.ToString()));
        var data = photoResult.Value!;

        return ApplicationExecuteLogicResult<DomainPhoto>.Success(new DomainPhoto
        {
            Id = metadata.Id,
            CarId = metadata.CarId,
            Extension = metadata.Extension,
            PhotoDataId = data.Id,
            PhotoData = data.PhotoBytes
        });
    }
    public async Task<ApplicationExecuteLogicResult<Unit>> DeleteCarPhoto(int carId)
    {
        logger.LogInformation("Удаление фото у машины {carId}", carId);
        
        //TODO: удаление фото по настоящему
        
        var carResult = await carRepository.GetCarByIdAsync(carId);
        if (carResult.IsSuccess is false)
            return ErrorHelper.WrapAllDbErrors<CarEntity, Unit>(CarErrors.CarNotFound, carResult, string.Join(" ", CarObjectName, carId.ToString()));
        var car = carResult.Value!;
        
        car.PhotoMetadata = null;
        
        var updatingResult = await carRepository.RewriteCarAsync(car);
        if (updatingResult.IsSuccess is false)
            return ErrorHelper.WrapAllDbErrors<CarEntity, Unit>(CarErrors.CarNotUpdated, carResult, string.Join(" ", CarObjectName, carId.ToString()));

        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
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