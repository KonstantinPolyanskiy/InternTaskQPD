namespace Models.Bridge.Photo;

/// <summary>
/// Способ получить фото (ссылку, хук на подготовку и скачивание, в base64 etc)
/// </summary>
public interface IPhotoAccessMethod
{
    public PhotoMethod Method { get; }
    public string Value { get; set; }
}

/// <summary>
/// Фотография автомобиля
/// </summary>
public interface ICarPhoto
{
    public int? Id { get; set; }
    public string? PhotoName { get; set; }
    
    /// <summary> Данные </summary>
    public PhotoData? Data { get; set; }
}

/// <summary>
/// Данные фото для сохранения
/// </summary>
public class PhotoData
{  
    /// <summary> Поток данных с фото </summary>
    public Stream? Content { get; set; }
    
    /// <summary> Расширение фотографии </summary>
    public string? FileExtension { get; set; }
}
