using Public.Models.BusinessModels.StorageModels;

namespace Public.Models.DtoModels.PhotoDtoModels;

public record DtoForCreatePhoto
{
    public ImageFileExtensions Extension { get; set; }
 
    public StorageTypes PriorityStorageType { get; set; }

    public required byte[] PhotoData { get; set; } 
    
    public int CarId { get; set; }
}