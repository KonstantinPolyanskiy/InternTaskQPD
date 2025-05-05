using Enum.Common;

namespace Backend.App.Models.Dto;

public record PhotoDto
{
    public Guid? Id { get; init; }
    
    public byte[]? Data { get; init; }
    
    public bool? UseOnlyPriorityStorage { get; init; }
    public PhotoStorageType? StorageType { get; init; }
    public PhotoFileExtension? Extension { get; init; }
}

public record PhotoMetadataDto
{
    public int? Id { get; init; }
    
    public int? CarId { get; init; }
    public Guid? PhotoId { get; init; }
    public PhotoStorageType? StorageType { get; set; }
    
    public PhotoFileExtension? Extension { get; init; }
}