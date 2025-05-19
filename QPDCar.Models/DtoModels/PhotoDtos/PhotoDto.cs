using QPDCar.Models.BusinessModels.PhotoModels;

namespace QPDCar.Models.DtoModels.PhotoDtos;

/// <summary> DTO для добавления в систему фотографии машины</summary>
public record DtoForAddPhoto
{
    public required byte[] Data { get; init; }
    public required string RawExtension { get; init; }
}

/// <summary> DTO для сохранения фотографии </summary>
public record DtoForSavePhoto
{
    public ImageFileExtensions Extension { get; set; }
 
    public PhotoStorageTypes PriorityStorageType { get; set; }

    public required byte[] PhotoData { get; set; } 
    
    public int CarId { get; set; }
}