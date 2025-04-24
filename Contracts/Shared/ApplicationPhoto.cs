namespace Contracts.Shared;

/// <summary>
/// Класс для работы с изображениями но уровне бизнес-логики
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