namespace Car.App.Models;

/// <summary>
/// Фото от DAL
/// </summary>
public class PhotoResult
{
    public int Id { get; set; }
    public StorageType Storage { get; set; }
    
    public string? Extension { get; set; }
    
    public int? RequestedCarId { get; set; }
    public string? Name { get; set; }
    public byte[]? Bytes { get; set; }
}

public class PhotoData
{
    public required int CarId { get; set; }
    public required string Extension {get; set;}
    
    public StorageType PriorityStorage {get; set;}
    public bool UseOnlyPriority {get; set;}
    public required byte[] Bytes { get; set; }
}