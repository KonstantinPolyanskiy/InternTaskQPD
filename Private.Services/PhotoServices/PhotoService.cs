using System.Net;
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
using Public.Models.DtoModels.PhotoDtoModels;

namespace Private.Services.PhotoServices;

public class PhotoService : IPhotoService
{
    private const string MetadataObjectName = "Photo metadata";
    private const string PhotoObjectName = "Photo image";
    private const string CarObjectName = "Car";
    
    private readonly ILogger<PhotoService> _logger;
    
    private readonly IPhotoRepository _photoRepository;
    private readonly IPhotoMetadataRepository _photoMetadataRepository;
    private readonly ICarRepository _carRepository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public PhotoService(IPhotoRepository photoRepository, IPhotoMetadataRepository photoMetadataRepository,
        ILogger<PhotoService> logger, ICarRepository carRepository)
    {
        _photoRepository = photoRepository;
        _photoMetadataRepository = photoMetadataRepository;
        _carRepository = carRepository;
        
        _logger = logger;
    }
    
    public async Task<ApplicationExecuteLogicResult<DomainPhoto>> CreatePhotoAsync(DtoForSavePhoto data)
    {
        _logger.LogInformation("Попытка создать фото в системе");
        _logger.LogDebug("Данные для создания - {data}", data);
        
        // Сохранить данные фото
        var savedImage = await _photoRepository.SavePhotoAsync(new PhotoEntity { PhotoBytes = data.PhotoData });
        if (savedImage.IsSuccess is not true)
        {
            if (savedImage.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<PhotoEntity, DomainPhoto>(PhotoImageErrors.ImageNotSaved, savedImage);
        }
        
        // сохранить метаданные
        var savedMetadata = await _photoMetadataRepository.SavePhotoMetadataAsync(new PhotoMetadataEntity
        {
            PhotoDataId = savedImage.Value!.Id,
            StorageType = data.PriorityStorageType,
            Extension = data.Extension,
            CarId = data.CarId,
        });
        if (savedImage.IsSuccess is not true)
        {
            if (savedImage.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<PhotoMetadataEntity, DomainPhoto>(PhotoMetadataErrors.MetadataNotSaved, savedMetadata);
        }
        
        // вернуть
        var photo = new DomainPhoto
        {
            CarId = savedMetadata.Value!.CarId,
            Extension = savedMetadata.Value!.Extension,
            Id = savedMetadata.Value!.Id,
            PhotoData = savedImage.Value!.PhotoBytes,
            PhotoDataId = savedImage.Value!.Id,
        };
        
        _logger.LogInformation("Фото с id {photoId} и метаданные к ним с id {metadataId} успешно сохранены", photo.Id, photo.PhotoDataId);
        
        return ApplicationExecuteLogicResult<DomainPhoto>.Success(photo);
    }

    public async Task<ApplicationExecuteLogicResult<DomainPhoto>> GetPhotoByCarIdAsync(int carId)
    {
        var car = await _carRepository.GetCarByIdAsync(carId);
        if (car.IsSuccess is not true)
            return ErrorHelper.WrapNotFoundError<CarEntity, DomainPhoto>(CarErrors.CarNotFound, CarObjectName, carId.ToString(), car);
        
        if (car.Value!.PhotoMetadataId is null)
            return ApplicationExecuteLogicResult<DomainPhoto>.Failure(new ApplicationError(CarErrors.CarNotFoundPhoto, "Нет метаданных",
                $"У машины с id {car.Value.Id!} нет метаданных фотографии", ErrorSeverity.Critical, HttpStatusCode.NotFound));

        var metadataResult = await _photoMetadataRepository.GetPhotoMetadataByIdAsync((int)car.Value!.PhotoMetadataId);
        if (metadataResult.IsSuccess is not true)
            return ErrorHelper.WrapNotFoundError<PhotoMetadataEntity, DomainPhoto>(
                PhotoMetadataErrors.MetadataNotFound, MetadataObjectName, car.Value!.PhotoMetadataId.Value.ToString(), metadataResult);
        var metadata = metadataResult.Value;
        
        var dataResult = await _photoRepository.GetPhotoByIdAsync((Guid)metadata!.PhotoDataId!);
        if (dataResult.IsSuccess is not true)
            return ErrorHelper.WrapNotFoundError<PhotoEntity, DomainPhoto>(PhotoImageErrors.ImageNotFound, PhotoObjectName, metadata.PhotoDataId.Value.ToString(), dataResult);
        var data = dataResult.Value;

        return ApplicationExecuteLogicResult<DomainPhoto>.Success(new DomainPhoto
        {
            Id = metadata.Id,
            CarId = metadata.CarId,
            Extension = metadata.Extension,
            PhotoDataId = data!.Id,
            PhotoData = data.PhotoBytes
        });
    }
}