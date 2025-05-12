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
    
    private readonly ILogger<PhotoService> _logger;
    
    private readonly IPhotoRepository _photoRepository;
    private readonly IPhotoMetadataRepository _photoMetadataRepository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public PhotoService(IPhotoRepository photoRepository, IPhotoMetadataRepository photoMetadataRepository,
        ILogger<PhotoService> logger)
    {
        _photoRepository = photoRepository;
        _photoMetadataRepository = photoMetadataRepository;
        _logger = logger;
    }
    
    public async Task<ApplicationExecuteLogicResult<DomainPhoto>> CreatePhotoAsync(DtoForCreatePhoto data)
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
        
        // TODO: добавить mapper
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

    public async Task<ApplicationExecuteLogicResult<DomainPhoto>> GetPhotoByMetadataIdAsync(int id)
    {
        _logger.LogInformation("Попытка найти фото по id метаданных {id}", id);
        
        var metadata = await _photoMetadataRepository.GetPhotoMetadataByIdAsync(id);
        if (metadata.IsSuccess is not true)
        {
            if (metadata.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<PhotoMetadataEntity, DomainPhoto>(PhotoMetadataErrors.MetadataNotFound, metadata);
            if (metadata.ContainsError(DatabaseErrors.NotFound))
                return ErrorHelper.WrapNotFoundError<PhotoMetadataEntity, DomainPhoto>(PhotoMetadataErrors.MetadataNotFound, MetadataObjectName, id.ToString(), metadata);
        }
        
        var photo = await _photoRepository.GetPhotoByIdAsync((Guid)metadata.Value!.PhotoDataId!);
        if (photo.IsSuccess is not true)
        {
            if (photo.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<PhotoEntity, DomainPhoto>(PhotoImageErrors.ImageNotFound, photo);
            if (photo.ContainsError(DatabaseErrors.NotFound))
                return ErrorHelper.WrapNotFoundError<PhotoEntity, DomainPhoto>(PhotoImageErrors.ImageNotFound, PhotoObjectName, id.ToString(), photo);
        }

        var domainPhoto = new DomainPhoto
        {
            Id = metadata.Value.Id,
            CarId = 0,
            Extension = metadata.Value!.Extension,
            PhotoDataId = photo.Value!.Id,
            PhotoData = photo.Value!.PhotoBytes,
        };
        
        _logger.LogInformation("По id метаданных {id} удалось найти фото", id);
        _logger.LogDebug("Найденное по id {id} фото {@data}", id, domainPhoto);
        
        return ApplicationExecuteLogicResult<DomainPhoto>.Success(domainPhoto);
    }
}