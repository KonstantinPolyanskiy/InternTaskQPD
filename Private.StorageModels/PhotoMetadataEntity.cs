using Public.Models.BusinessModels.StorageModels;

namespace Private.StorageModels;

/// <summary> Таблица PhotoMetadata для хранения метаинформации о фотографии </summary>
public class PhotoMetadataEntity
{
    public int Id { get; set; }
    
    public Guid? PhotoDataId { get; set; }
    
    public StorageTypes StorageType { get; set; }

    public ImageFileExtensions Extension { get; set; }
    
    public int CarId { get; set; }
    
    public CarEntity Car { get; set; } = null!;
}