using Enum.Common;

namespace Backend.Api.Models.Responses;

public record PhotoResponse
{
    public Guid Id { get; init; }
    public PhotoFileExtension Extension { get; init; }
    public string? AccessMethod { get; set; }
    public string? Access { get; set; }
}