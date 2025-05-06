using AutoMapper;
using Backend.App.Models.Business;
using Backend.App.Models.Commands;
using Backend.App.Models.Dto;
using Backend.App.Repositories;
using Enum.Common;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Backend.App.Services.PhotoService;

public class InternalPhotoService(
    IPhotoRepository photoRepository,
    ICarRepository carRepository,
    IPhotoMetadataRepository photoMetadataRepository,
    IMapper mapper,
    ILogger<InternalPhotoService> log)
{
    internal async Task<Photo> SetPhotoToCarAsync(SetPhotoToCarCommand cmd)
    {
        log.LogInformation("Попытка установить машине {@carId} фото {@photo}", cmd.CarId, cmd.RawExtension);

        // сохранить фото
        var ext = PhotoFileExtensionHelper.MapExtension(cmd.RawExtension);

        var photoDto = new PhotoDto
        {
            Data = cmd.Data,
            Extension = ext,
            UseOnlyPriorityStorage = true,
            StorageType = PhotoStorageType.Minio
        };

        log.LogDebug("Данные сохраняемого фото - {@photoData}", photoDto);

        var savedPhotoDto = await photoRepository.SavePhotoAsync(photoDto);
        if (savedPhotoDto is null) throw new ApplicationException("не получилось сохранить фото");

        log.LogInformation("Фото {@photoId} успешно сохранено", savedPhotoDto.Id);

        // сохранить метаданные
        var metadataDto = new PhotoMetadataDto
        {
            CarId = cmd.CarId,
            PhotoId = savedPhotoDto.Id,
            StorageType = photoDto.StorageType,
            Extension = ext
        };

        log.LogDebug("Данные сохраняемой метаинформации фото - {@metadata}", metadataDto);

        var savedMetadataDto = await photoMetadataRepository.SavePhotoMetadataAsync(metadataDto);
        if (savedMetadataDto is null) throw new ApplicationException("не получилось сохранить данные о фото");

        log.LogInformation("Метаинформация с id {@metadataId} успешно сохранена", savedMetadataDto.Id);

        // Обновить машину
        var carDto = new CarDto
        {
            Id = cmd.CarId,
            PhotoMetadataId = savedMetadataDto.Id,
        };

        log.LogDebug("Данные обновляемой машины - {@carData}", carDto);

        var updatedCarDto = await carRepository.UpdateCarAsync(carDto);
        if (updatedCarDto is null) throw new ApplicationException("не удалось обновить фото у машины");

        log.LogInformation("Машина с id - {@carId} успешно обновлена", updatedCarDto.Id);

        return new Photo
        {
            Id = (Guid)savedPhotoDto.Id!,
            Storage = (PhotoStorageType)savedMetadataDto.StorageType!,
            Data = new PhotoData
            {
                Extension = (PhotoFileExtension)savedMetadataDto.Extension!,
                Data = savedPhotoDto.Data!,
            },
        };
    }

    public async Task<Photo?> GetPhotoByMetadataIdAsync(SearchPhotoByMetadataIdCommand cmd)
    {
        log.LogInformation("Попытка получить фото c id метаданных");
        log.LogDebug("Данные для получения - {@metadata}", cmd);

        var metadataDto = await photoMetadataRepository.GetPhotoMetadataAsync(cmd.MetadataId);
        if (metadataDto is null) return null;

        var photoDto =
            await photoRepository.GetPhotoAsync((Guid)metadataDto.PhotoId!, (PhotoStorageType)metadataDto.StorageType!);
        if (photoDto is null) return null;

        log.LogInformation("Фото успешно получено");

        return new Photo
        {
            Id = (Guid)photoDto.Id!,
            Storage = (PhotoStorageType)metadataDto.StorageType!,
            Data = new PhotoData
            {
                Extension = (PhotoFileExtension)photoDto.Extension!,
                Data = photoDto.Data!,
            },
        };
    }

    public async Task DeletePhotoByMetadataIdAsync(int id)
    {
        log.LogInformation("Попытка удалить фото по id метаданных");
        log.LogDebug("id метаданных - {metadataId}", id);

        var metadataDto = await photoMetadataRepository.GetPhotoMetadataAsync(id);
        if (metadataDto?.PhotoId is null) return;

        await photoRepository.DeletePhotoAsync((Guid)metadataDto.PhotoId, (PhotoStorageType)metadataDto.StorageType!);
    }

    private async Task CheckExistingPhotoAndDeleteIfExistAsync(int carId)
    {
        log.LogInformation("Попытка найти фото для машины с id - {carId}", carId);
        
        log.LogInformation("Поиск машины с id {carId}", carId);
        var car = await carRepository.GetCarByIdAsync(carId);
        if (car is null)
        {
            log.LogInformation("В процессе проверки существования фото у машины с id {carId} машина была не найдена", carId);
            return;
        }

        log.LogInformation("Машина с id {carId} найдена", carId);
        
        if (car.PhotoMetadataId is null)
        {
            log.LogInformation("У найденной машины {carId} отсутствуют метаданные о фото", carId);
            return;
        }
        
        log.LogInformation("Поиск метаданных с id - {metadataId}", car.PhotoMetadataId);
        var metadata = await photoMetadataRepository.GetPhotoMetadataAsync((int)car.PhotoMetadataId);
        if (metadata is null)
        {
            log.LogInformation("Метаданные фото по id {metadataId} не были найдены", car.PhotoMetadataId);
            return;
        }
        
        log.LogInformation("Метаданные с id {metadata} найдены", metadata.Id);

        if (metadata.PhotoId is null || metadata.StorageType is null)
        {
            log.LogInformation("В метаданных {metadataId} недостаточно информации о фото, метаданные- {@data}", metadata.Id, metadata);
            return;
        }
        
        log.LogInformation("Поиск фото с id {@photo}, хранилище - {@storage}", metadata.PhotoId, metadata.StorageType);
        var photo = await photoRepository.GetPhotoAsync((Guid)metadata.PhotoId, (PhotoStorageType)metadata.StorageType);
        if (photo is null)
        {
            log.LogInformation("Фото с id {@photo} не было найдено", metadata.PhotoId);
            return;
        }
        
        log.LogInformation("Фото с id {photo} найдено", photo.Id);

        await CascadeDeleteAsync();
    }
    
    private async Task<CarDto> GetCar(int carId)
    private async Task CascadeDeleteAsync(Guid photoId, PhotoStorageType storage, int carId, int metadataId)
    {
        log.LogInformation("Удаление фото {@id}", photoId);
        await photoRepository.DeletePhotoAsync(photoId, storage);
        
        log.LogInformation("Удаление метаданных у машины {id}", carId);
        await carRepository.DeleteMetadataAsync(carId);
        
        log.LogInformation("Удаление метаданных {id}", metadataId);
        await photoMetadataRepository.DeleteMetadataAsync(metadataId);
    }

}