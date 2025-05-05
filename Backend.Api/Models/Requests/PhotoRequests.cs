namespace Backend.Api.Models.Requests;

/// <summary> Запрос на установку фото для машины </summary>
public record SetPhotoToCarRequest
{
    public required IFormFile Photo { get; init; }
}