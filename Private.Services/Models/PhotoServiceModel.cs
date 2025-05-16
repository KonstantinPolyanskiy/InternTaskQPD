using Private.StorageModels;

namespace Private.Services.Models;

public class PhotoServiceModel
{
    public PhotoMetadataEntity? Metadata { get; set; }
    public PhotoEntity? Image { get; set; }
}