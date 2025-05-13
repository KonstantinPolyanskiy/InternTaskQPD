using Public.Models.BusinessModels.StorageModels;

namespace Public.Models.DtoModels.PhotoDtoModels;

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
 
    public StorageTypes PriorityStorageType { get; set; }

    public required byte[] PhotoData { get; set; } 
    
    public int CarId { get; set; }
}

