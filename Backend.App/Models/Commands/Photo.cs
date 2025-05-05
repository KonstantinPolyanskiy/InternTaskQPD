using Enum.Common;

namespace Backend.App.Models.Commands;

public record SetPhotoToCarCommand
{
    public required int CarId { get; init; }
    
    public required string RawExtension { get; init; }
    
    public required byte[] Data { get; init; }
}

public record SearchPhotoByMetadataIdCommand
{
    public required int MetadataId { get; init; }
}