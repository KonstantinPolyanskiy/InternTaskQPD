namespace Private.Jobs.Models;

/// <summary> Данные для события с машинами без фото </summary>
public record MissingPhotoEvent
{
    public List<(int CarId, string ManagerMail)> CarsData { get; init; } = [];
}