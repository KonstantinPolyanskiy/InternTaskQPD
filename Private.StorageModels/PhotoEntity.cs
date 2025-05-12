namespace Private.StorageModels;

/// <summary> Таблица Photo для непосредственного хранения фото </summary>
public class PhotoEntity()
{
    public Guid Id { get; init; }
    
    public byte[] PhotoBytes { get; init; } = null!;
}