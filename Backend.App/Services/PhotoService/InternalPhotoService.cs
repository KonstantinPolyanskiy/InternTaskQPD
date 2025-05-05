using AutoMapper;
using Backend.App.Models.Business;
using Backend.App.Models.Commands;
using Backend.App.Models.Dto;
using Backend.App.Repositories;
using Enum.Common;

namespace Backend.App.Services.PhotoService;

public class InternalPhotoService(
    IMapper mapper,
    IPhotoRepository photoRepository, ICarRepository carRepository,
    IPhotoMetadataRepository photoMetadataRepository)
{
    public async Task<Photo> SetPhotoToCarAsync(SetPhotoToCarCommand cmd)
    {
        // сохранить фото
        var ext = PhotoFileExtensionHelper.MapExtension(cmd.RawExtension);
        
        var photoDto = new PhotoDto
        {
            Data = cmd.Data,
            Extension = ext,
            UseOnlyPriorityStorage = true,
            StorageType = PhotoStorageType.Minio
        };
        
        var savedPhotoDto = await photoRepository.SavePhotoAsync(photoDto);
        if (savedPhotoDto is null) throw new ApplicationException("не получилось сохранить фото");
        
        // сохранить метаданные
        var metadataDto = new PhotoMetadataDto
        {
            CarId = cmd.CarId,
            PhotoId = savedPhotoDto.Id,
            StorageType = photoDto.StorageType,
            Extension = ext
        };
        
        var savedMetadataDto = await photoMetadataRepository.SavePhotoMetadataAsync(metadataDto);
        if (savedMetadataDto is null) throw new ApplicationException("не получилось сохранить данные о фото");
        
        // Обновить машину
        var carDto = new CarDto
        {
            Id = cmd.CarId,
            PhotoMetadataId = savedMetadataDto.Id,
        };
        
        var updatedCarDto = await carRepository.UpdateCarAsync(carDto);
        if (updatedCarDto is null) throw new ApplicationException("не удалось обновить фото у машины");

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
        var metadataDto = await photoMetadataRepository.GetPhotoMetadataAsync(cmd.MetadataId);
        if (metadataDto is null) return null;
        
        var photoDto = await photoRepository.GetPhotoAsync((Guid)metadataDto.PhotoId!, (PhotoStorageType)metadataDto.StorageType!);
        if (photoDto is null) return null;
        
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
        
    }
}