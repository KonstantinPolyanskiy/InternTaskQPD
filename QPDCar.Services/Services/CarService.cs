using System.Net;
using Microsoft.Extensions.Logging;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ApplicationResult.Extensions;
using QPDCar.Models.ApplicationModels.ErrorTypes;
using QPDCar.Models.BusinessModels.CarModels;
using QPDCar.Models.BusinessModels.EmployerModels;
using QPDCar.Models.BusinessModels.PhotoModels;
using QPDCar.Models.DtoModels.CarDtos;
using QPDCar.Models.DtoModels.PhotoDtos;
using QPDCar.Models.StorageModels;
using QPDCar.ServiceInterfaces;
using QPDCar.ServiceInterfaces.UserServices;
using QPDCar.Services.ErrorHelpers;
using QPDCar.Services.Repositories;

namespace QPDCar.Services.Services;

public class CarService(ICarRepository carRepo, IPhotoDataRepository photoDataRepo, IPhotoMetadataRepository photoMetadataRepo,
    ILogger<CarService> logger, IEmployerService employerService) : ICarService
{
    private const string CarObjectName = "Car";
    private const string MetadataObjectName = "Photo Metadata";
    private const string DataObjectName = "Photo Data"; 
    
    public async Task<ApplicationExecuteResult<DomainCar>> CreateCarAsync(DtoForSaveCar carData)
    {
        logger.LogInformation("Создание машины с данными {@carData}", carData);

        var warns = new List<ApplicationError>();

        PhotoDataEntity? photoData = null;
        PhotoMetadataEntity? photoMetadata = null;
        
        var condition = CalculateCarCondition(carData.CurrentOwner, carData.Mileage);
        var priority = CalculatePrioritySale(condition);
        
        var carResult = await carRepo.SaveAsync(new CarEntity
        {
            Brand = carData.Brand,
            Color = carData.Color,
            Price = carData.Price,
            CurrentOwner = carData.CurrentOwner,
            Mileage = carData.Mileage,
            PrioritySale = priority,
            CarCondition = condition,
            ResponsiveManagerId = carData.ResponsiveManager,
        });
        if (carResult.IsSuccess is not true)
            return DbErrorHelper.WrapAllDbErrors<CarEntity, DomainCar>(CarErrors.CarNotSaved, carResult, string.Join(" ", CarObjectName));
        var carEntity = carResult.Value!;
        
        // Если есть фото - сохраняем изображение и метаданные, обновляем машину
        if (carData.Photo != null)
        {
            var dataResult = await photoDataRepo.SaveAsync(new PhotoDataEntity { PhotoBytes = carData.Photo.PhotoData, });
            if (dataResult.IsSuccess is false)
                warns.Add(new ApplicationError(
                    PhotoErrors.PhotoDataNotSaved, "Фото не сохранено",
                    "Данные фото не сохранены для добавляемой машины",
                    ErrorSeverity.NotImportant));
            photoData = dataResult.Value!;
            
            var metadataResult = await photoMetadataRepo.SaveAsync(new PhotoMetadataEntity
            {
                PhotoDataId = photoData.Id,
                StorageType = carData.Photo.PriorityStorageType,
                Extension = carData.Photo.Extension,
                CarId = carEntity.Id,
            });
            if (metadataResult.IsSuccess is false)
                warns.Add(new ApplicationError(
                    PhotoErrors.MetadataNotSaved, "Фото не сохранено",
                    "Метаданные изображения не сохранены",
                    ErrorSeverity.NotImportant));
            photoMetadata = metadataResult.Value!;
        }
        
        logger.LogInformation("Машина с id {id} успешно сохранена", carEntity.Id);
        
        var managerResult = await employerService.ManagerByCarId(carEntity.Id);
        if (managerResult.IsSuccess is false)
            return ApplicationExecuteResult<DomainCar>.Failure().Merge(managerResult);
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
            Photo = photoData != null && photoMetadata != null ? new DomainPhoto
                {
                    Id = photoMetadata.Id,
                    CarId = carEntity.Id,
                    Extension = photoMetadata.Extension,
                    PhotoDataId = photoData.Id,
                    PhotoData = photoData.PhotoBytes
                }
                : null, 
        };
        
        return ApplicationExecuteResult<DomainCar>.Success(car).WithWarnings(warns);
    }

    public async Task<ApplicationExecuteResult<DomainCar>> UpdateCarAsync(DtoForUpdateCar carData)
    {
        logger.LogInformation("Попытка обновить данные машины {@carData}", carData);
        
        // Получаем старую машину
        var oldCarResult = await carRepo.ByIdAsync(carData.CarId);
        if (oldCarResult.IsSuccess is false)
            return DbErrorHelper.WrapAllDbErrors<CarEntity, DomainCar>(CarErrors.CarNotUpdated, oldCarResult, string.Join(" ", CarObjectName, carData.CarId.ToString()));
        var oldCar = oldCarResult.Value!;
        
        oldCar.Brand = carData.Brand;
        oldCar.Color = carData.Color;
        oldCar.Price = carData.Price;
        
        if (carData.NewManager != null)
            oldCar.ResponsiveManagerId = (Guid)carData.NewManager;
        
        // Перезаписываем машину
        var updatedCarResult = await carRepo.RewriteAsync(oldCar);
        if (updatedCarResult.IsSuccess is false)
            return DbErrorHelper.WrapAllDbErrors<CarEntity, DomainCar>(CarErrors.CarNotUpdated, updatedCarResult, string.Join(" ", CarObjectName, carData.CarId.ToString()));
        var updatedCar = updatedCarResult.Value!;
        
        // Собираем машину со всеми полями
        var warns = new List<ApplicationError>();
        
        var managerResult = await employerService.ManagerByCarId(carData.CarId);
        if (managerResult.IsSuccess is false)
            warns.Add(new ApplicationError(
                UserErrors.UserNotFound, "Менеджер машины не найден",
                $"Не получилось найти менеджера машины {updatedCar.Id}",
                ErrorSeverity.NotImportant));
        var manager = managerResult.Value!;
        
        var photoResult = await PhotoByCarIdAsync(carData.CarId);
        if (photoResult.IsSuccess is false)
            warns.Add(new ApplicationError(
                PhotoErrors.PhotoDataNotSaved, "Фото машины не найдено",
                $"Не получилось найти фото машины {updatedCar.Id}",
                ErrorSeverity.NotImportant));
        var photo = photoResult.Value!;

        return ApplicationExecuteResult<DomainCar>.Success(new DomainCar
            {
                Id = updatedCar.Id,
                Brand = updatedCar.Brand,
                Color = updatedCar.Color,
                Price = updatedCar.Price,
                CurrentOwner = updatedCar.CurrentOwner,
                Mileage = updatedCar.Mileage,
                IsSold = updatedCar.IsSold,
                CarCondition = updatedCar.CarCondition,
                PrioritySale = updatedCar.PrioritySale,
                Manager = manager,
                Photo = photo
            })
            .WithWarnings(warns);
    }

    public async Task<ApplicationExecuteResult<Unit>> DeleteCarAsync(int carId)
    {
        logger.LogInformation("Удаление машины {carId}", carId);
        
        // TODO: удаление фото и тд
        
        var result = await carRepo.DeleteAsync(carId);
        if (result.IsSuccess is false)
            return DbErrorHelper.WrapAllDbErrors<Unit, Unit>(CarErrors.CarNotDeleted, result, string.Join(" ", CarObjectName, carId.ToString()));
        
        return ApplicationExecuteResult<Unit>.Success(Unit.Value);
    }

    public async Task<ApplicationExecuteResult<DomainCar>> SoldCarAsync(DomainCar car)
    {
        logger.LogInformation("Продажа машины с id {carId}", car.Id);

        var carEntityResult = await carRepo.ByIdAsync(car.Id);
        if (carEntityResult.IsSuccess is false)
            return ApplicationExecuteResult<DomainCar>.Failure(new ApplicationError(
                CarErrors.CarNotUpdated, "Машина не продана",
                "Ошибка при обновлении статуса машины на проданную",
                ErrorSeverity.Critical, HttpStatusCode.InternalServerError));
        var entity = carEntityResult.Value!;
        
        entity.IsSold = true;
        entity.PrioritySale = PrioritySaleTypes.No;
        
        var updatedResult = await carRepo.RewriteAsync(entity);
        if (updatedResult.IsSuccess is false)
            return DbErrorHelper.WrapAllDbErrors<CarEntity, DomainCar>(CarErrors.CarNotUpdated, updatedResult, string.Join(" ", CarObjectName, car.Id.ToString()));

        car.IsSold = true;
        car.PrioritySale = PrioritySaleTypes.No;

        return ApplicationExecuteResult<DomainCar>.Success(car);
    }

    public async Task<ApplicationExecuteResult<DomainCar>> ByIdAsync(int carId)
    {
        logger.LogInformation("Получение машины с id {carId}", carId);
        
        // Находим машину
        var carEntityResult = await carRepo.ByIdAsync(carId);
        if (carEntityResult.IsSuccess is false)
            return DbErrorHelper.WrapAllDbErrors<CarEntity, DomainCar>(CarErrors.CarNotFound, carEntityResult, string.Join(" ", CarObjectName, carId.ToString()));
        var carEntity = carEntityResult.Value!;
        
        var warns = new List<ApplicationError>();
        
        // Находим менеджера
        var managerResult = await employerService.ManagerByCarId(carId);
        if (managerResult.IsSuccess is false)
            warns.Add(new ApplicationError(
                UserErrors.UserNotFound, "Менеджер машины не найден",
                $"Не получилось найти менеджера машины {carEntity.Id}",
                ErrorSeverity.NotImportant));
        var manager = managerResult.Value!;
        
        // Находим фото
        var photoResult = await PhotoByCarIdAsync(carId);
        if (managerResult.IsSuccess is false)
            warns.Add(new ApplicationError(
                PhotoErrors.MetadataNotSaved, "Фото машины не найдено",
                $"Не получилось найти фото машины {carEntity.Id}",
                ErrorSeverity.NotImportant));
        var photo = photoResult.Value!;

        return ApplicationExecuteResult<DomainCar>.Success(new DomainCar
            {
                Id = carEntity.Id,
                Brand = carEntity.Brand,
                Color = carEntity.Color,
                Price = carEntity.Price,
                CurrentOwner = carEntity.CurrentOwner,
                Mileage = carEntity.Mileage,
                IsSold = carEntity.IsSold,
                CarCondition = carEntity.CarCondition,
                PrioritySale = carEntity.PrioritySale,
                Manager = manager,
                Photo = photo
            })
            .WithWarnings(warns);
    }

    public async Task<ApplicationExecuteResult<DomainCarPage>> ByParamsAsync(DtoForSearchCars parameters)
    {
        logger.LogInformation("Попытка найти машины по параметрам {@data}", parameters);
        
        var entityPageResult = await carRepo.ByParamsAsync(parameters);
        if (entityPageResult.IsSuccess is false)
            return DbErrorHelper.WrapAllDbErrors<CarEntityPage, DomainCarPage>(CarErrors.CarNotFound, entityPageResult, 
                $"Множество {CarObjectName} по параметрам {@parameters}");
        var entityPage = entityPageResult.Value!;
        
        var warns = new List<ApplicationError>();
        var cars = new List<DomainCar>();
        
        foreach (var entity in entityPage.Cars)
        {
            var carResult = await ByIdAsync(entity.Id);
            warns.AddRange(carResult.GetWarnings);
            
            cars.Add(carResult.Value!);
        }
        
        return ApplicationExecuteResult<DomainCarPage>.Success(new DomainCarPage
        {
            Cars = cars,
            TotalCount = entityPage.TotalCount,
            PageNumber = entityPage.PageNumber,
            PageSize = entityPage.PageSize,
        }).WithWarnings(warns);
    }

    public async Task<ApplicationExecuteResult<DomainPhoto>> SetCarPhotoAsync(int carId, DtoForSavePhoto photoDto)
    {
        logger.LogInformation("Попытка установить фото машине {carId}", carId);
        
        // Получаем обновляемую машину
        var oldCarResult = await carRepo.ByIdAsync(carId);
        if (oldCarResult.IsSuccess is false)
            return DbErrorHelper.WrapAllDbErrors<CarEntity, DomainPhoto>(CarErrors.CarNotSetPhoto, oldCarResult, string.Join(" ", CarObjectName, carId.ToString())); 
        var oldCar = oldCarResult.Value!;
        
        // Сохраняем байты
        var dataResult = await photoDataRepo.SaveAsync(new PhotoDataEntity { PhotoBytes = photoDto.PhotoData});
        if (dataResult.IsSuccess is false)
            return DbErrorHelper.WrapAllDbErrors<PhotoDataEntity, DomainPhoto>(CarErrors.CarNotSetPhoto, dataResult, string.Join(" ", CarObjectName, carId.ToString()));
        var data = dataResult.Value!;
        
        // Сохраняем метаданные
        var metadataResult = await photoMetadataRepo.SaveAsync(new PhotoMetadataEntity
        {
            PhotoDataId = data.Id,
            StorageType = photoDto.PriorityStorageType,
            Extension = photoDto.Extension,
            CarId = carId,
        });
        if (metadataResult.IsSuccess is false)
            return DbErrorHelper.WrapAllDbErrors<PhotoMetadataEntity, DomainPhoto>(CarErrors.CarNotSetPhoto, metadataResult, string.Join(" ", CarObjectName, carId.ToString()));
        var metadata = metadataResult.Value!;
        
        // Обновляем машину с установленным фото
        oldCar.PhotoMetadataId = metadata.Id;
        
        var updatedCarResult = await carRepo.RewriteAsync(oldCar);
        if (updatedCarResult.IsSuccess is false)
            return DbErrorHelper.WrapAllDbErrors<CarEntity, DomainPhoto>(CarErrors.CarNotSetPhoto, updatedCarResult, string.Join(" ", CarObjectName, carId.ToString()));
        var car = updatedCarResult.Value!;

        return ApplicationExecuteResult<DomainPhoto>.Success(new DomainPhoto
        {
            Id = metadata.Id,
            CarId = car.Id,
            Extension = metadata.Extension,
            PhotoDataId = data.Id,
            PhotoData = data.PhotoBytes
        });
    }

    public async Task<ApplicationExecuteResult<DomainCar>> DeleteCarPhotoAsync(int carId)
    {
        logger.LogInformation("Удаление фото у машины {carId}", carId);
        
        var oldCarResult = await carRepo.ByIdAsync(carId);
        if (oldCarResult.IsSuccess is false)
            return DbErrorHelper.WrapAllDbErrors<CarEntity, DomainCar>(CarErrors.CarNotUpdated, oldCarResult, string.Join(" ", CarObjectName, carId.ToString()));
        var oldCar = oldCarResult.Value!;
        
        oldCar.PhotoMetadataId = null;
        
        var updatedCarResult = await carRepo.RewriteAsync(oldCar);
        if (updatedCarResult.IsSuccess is false)
            return DbErrorHelper.WrapAllDbErrors<CarEntity, DomainCar>(CarErrors.CarNotUpdated, updatedCarResult, string.Join(" ", CarObjectName, carId.ToString()));
        var updatedCar = updatedCarResult.Value!;
        
        var carResult = await ByIdAsync(updatedCar.Id);
        if (carResult.IsSuccess is false)
            return ApplicationExecuteResult<DomainCar>.Failure().Merge(carResult);
        var car = carResult.Value!;

        return ApplicationExecuteResult<DomainCar>.Success(car);
    }
    public async Task<ApplicationExecuteResult<DomainPhoto>> PhotoByCarIdAsync(int carId)
    {
        logger.LogInformation("Получение фото машины {carId}", carId);
        
        var carResult =  await carRepo.ByIdAsync(carId);
        if (carResult.IsSuccess is false)
            return DbErrorHelper.WrapAllDbErrors<CarEntity, DomainPhoto>(PhotoErrors.PhotoNotFound, carResult, string.Join(" ", CarObjectName, carId.ToString()));
        var car = carResult.Value!;

        var metadataResult = await photoMetadataRepo.ByIdAsync((int)car.PhotoMetadataId!);
        if (metadataResult.IsSuccess is false)
            return DbErrorHelper.WrapAllDbErrors<PhotoMetadataEntity, DomainPhoto>(PhotoErrors.PhotoNotFound, metadataResult, string.Join(" ", MetadataObjectName, car.PhotoMetadataId.ToString()));
        var metadata = metadataResult.Value!;

        var dataResult = await photoDataRepo.ByIdAsync((Guid)metadata.PhotoDataId!);
        if (dataResult.IsSuccess is false)
            return DbErrorHelper.WrapAllDbErrors<PhotoDataEntity, DomainPhoto>(PhotoErrors.PhotoNotFound, dataResult, string.Join(" ", DataObjectName, metadata.PhotoDataId.ToString()));
        var data = dataResult.Value!;

        return ApplicationExecuteResult<DomainPhoto>.Success(new DomainPhoto
        {
            Id = metadata.Id,
            CarId = car.Id,
            Extension = metadata.Extension,
            PhotoDataId = data.Id,
            PhotoData = data.PhotoBytes
        });
    }
    
    #region Вспомогательные методы

    private static ConditionTypes CalculateCarCondition(string? owner, int? mileage)
    {
        if (string.IsNullOrWhiteSpace(owner) && mileage is null or <= 0)
            return ConditionTypes.New;
        
        if (mileage >= 10000)
            return ConditionTypes.NotWork;
        
        return ConditionTypes.Used;
    }

    private static PrioritySaleTypes CalculatePrioritySale(ConditionTypes carCondition)
    {
        return carCondition switch
        {
            ConditionTypes.New => PrioritySaleTypes.High,
            ConditionTypes.Used => PrioritySaleTypes.Medium,
            ConditionTypes.NotWork => PrioritySaleTypes.Low,
            _ => PrioritySaleTypes.Unknown
        };
    }

    #endregion
}