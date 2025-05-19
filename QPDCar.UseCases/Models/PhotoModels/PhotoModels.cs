using QPDCar.Models.BusinessModels.PhotoModels;

namespace QPDCar.UseCases.Models.PhotoModels;

public class PhotoUseCaseResponse
{
    public int? MetadataId { get; set; }
    
    public ImageFileExtensions? Extension { get; set; }
    
    public Guid? PhotoDataId { get; set; }
    
    public byte[]? PhotoBytes { get; set; }
}