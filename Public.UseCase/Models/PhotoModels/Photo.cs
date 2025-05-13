using Public.Models.BusinessModels.StorageModels;

namespace Public.UseCase.Models.PhotoModels;

public class PhotoUseCaseResponse
{
    public int? MetadataId { get; set; }
    
    public ImageFileExtensions? Extension { get; set; }
    
    public Guid? PhotoDataId { get; set; }
    
    public byte[]? PhotoBytes { get; set; }
}