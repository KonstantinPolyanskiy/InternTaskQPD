using Car.App.Models.CarModels;

namespace Car.App.Models.PhotoModels;

/// <summary> Данные фото </summary>
public class PhotoData
{
    public required int CarId { get; set; }
    public required string Extension {get; set;}
    
    public PhotoStorageType? PriorityPhotoStorage {get; set;}
    public bool? UseOnlyPriority {get; set;}

    public Stream? Content { get; set; }
}

/// <summary> Фото от DAL </summary>
public class PhotoResult
{
    public int Id { get; set; }
    public PhotoStorageType PhotoStorage { get; set; }
    
    public string? Extension { get; set; }
    
    public int? RequestedCarId { get; set; }
    public string? Name { get; set; }
    public byte[]? Bytes { get; set; }
}

/// <summary> Данные для добавления машине фото </summary> 
public class PhotoRequest
{
    public required int CarId { get; set; }
    public required string Extension { get; set; }
    public PhotoStorageType PriorityPhotoStorage { get; set; }
    public Stream? Content { get; set; }
}