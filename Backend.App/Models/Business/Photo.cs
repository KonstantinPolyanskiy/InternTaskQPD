using Enum.Common;

namespace Backend.App.Models.Business;

public class Photo
{
    public Guid Id { get; set; }
    
    public PhotoStorageType Storage { get; set; }
    
    public PhotoData Data { get; set; }
    
    public IPhotoAccessor? PhotoAccessor { get; set; }
}

/// <summary> Сами данные фотографии </summary>
public class PhotoData
{
    public PhotoFileExtension Extension { get; set; }
    
    public required byte[] Data { get; set; }
}

/// <summary> Способ получить данные фотографии </summary>
public interface IPhotoAccessor
{
    public PhotoMethod AccessMethod { get; }
    public string Access();
}