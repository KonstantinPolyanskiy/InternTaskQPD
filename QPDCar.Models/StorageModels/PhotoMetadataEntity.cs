using QPDCar.Models.BusinessModels.PhotoModels;

namespace QPDCar.Models.StorageModels;

/// <summary> Описание таблицы с метаданными фотографии </summary>
public class PhotoMetadataEntity()
{
    public int Id { get; set; }
    
    public Guid? PhotoDataId { get; set; }
    
    public PhotoStorageTypes StorageType { get; set; }

    public ImageFileExtensions Extension { get; set; }
    
    public int CarId { get; set; }
    
    public CarEntity Car { get; set; } = null!;
}