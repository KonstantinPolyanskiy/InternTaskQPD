using Public.Models.BusinessModels.StorageModels;

namespace Public.UseCase.Models;


public record PhotoDataResponse
{
    public Guid Id { get; init; }
    public required byte[] Data { get; init; }
}

public record PhotoMetadataResponse
{
    public int Id { get; init; }
    public ImageFileExtensions Extension { get; init; }
}

public record PhotoResponse
{
    public PhotoMetadataResponse Metadata { get; init; } = null!;
    
    public PhotoDataResponse Image { get; init; } = null!;
}