using Car.App.Models.CarModels;

namespace Car.App.Models.Dto;

/// <summary>
/// Данные для сохранения фото
/// </summary>
public record PhotoDataDto
{
    public required byte[] PhotoBytes { get; set; }
    public required string Extension {get; set;}
    
    public PhotoStorageType? PriorityPhotoStorage {get; set;}
}

/// <summary>
/// Данные сохраненного фото
/// </summary>
public record PhotoResultDto
{
    public required string TermId { get; set; }
    public int? CarId { get; set; }
    
    public PhotoStorageType PhotoStorage { get; set; }

    public byte[]? Bytes { get; set; }
    public string? Name { get; set; }
    public string? Extension { get; set; }
    
}

/// <summary>
/// Данные для создания фото в системе и назначения его машине
/// </summary>
public record PhotoRequestDto
{
    public required int CarId {get; set;}
    public required string PhotoExtension {get; set;}
    public PhotoStorageType? PriorityPhotoStorage {get; set;}
    public required Stream Content {get; set;}
}