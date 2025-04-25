namespace Contracts.Shared;

/// <summary>
/// Данные файла для сохранения
/// </summary>
public class ApplicationPhotoModel
{  
    /// <summary> Поток данных с фото </summary>
    public Stream? Content { get; set; }
    
    /// <summary> Название файла </summary>
    public string? FileName { get; set; }
    
    /// <summary> Расширение фотографии </summary>
    public string? FileExtension { get; set; }
    
    /// <summary> Размер фото </summary>
    public long Length { get; set; }
}

/// <summary>
/// Фотография автомобиля
/// </summary>
public interface ICarPhoto
{
    public int? Id { get; set; }
    public string? PhotoName { get; set; }
    /// <summary> Фото </summary>
    public ApplicationPhotoModel? Model { get; set; }
}

/// <summary>
/// Способ получить фото (ссылку, хук на подготовку и скачивание, в base64 etc)
/// </summary>
public interface IPhotoGetter
{
    public string WayToGet { get; set; }
}

public class CarPhoto : ICarPhoto
{
    public int? Id { get; set; }
    public string? PhotoName { get; set; }
    public ApplicationPhotoModel? Model { get; set; }
} 