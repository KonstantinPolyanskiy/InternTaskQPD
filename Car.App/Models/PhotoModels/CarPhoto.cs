using Car.App.Models.Photo;

namespace Car.App.Models.PhotoModels;

public class CarPhoto : ICarPhoto, IPhotoAccessMethod
{
    #region ICarPhoto

    public string? Id { get; set; }
    
    public string? PhotoName { get; set; }
    
    public PhotoData? Data { get; set; }
    
    #endregion

    public PhotoMethod Method { get; set; }
    public string Value { get; set; }
} 

/// <summary> Cпособ получить фото (ссылку, хук на подготовку и скачивание, в base64 etc) </summary>
public interface IPhotoAccessMethod
{
    public PhotoMethod Method { get; }
    public string Value { get; set; }
}

/// <summary> Фотография автомобиля </summary>
public interface ICarPhoto
{
    public string? Id { get; set; }
    public string? PhotoName { get; set; }
    
    /// <summary> Данные </summary>
    public PhotoData? Data { get; set; }
}
