namespace QPDCar.Models.StorageModels;

/// <summary> Описание таблицы с байтами изображения </summary>
public class PhotoDataEntity()
{
    public Guid Id { get; init; }
    
    public byte[] PhotoBytes { get; init; } = null!;
}