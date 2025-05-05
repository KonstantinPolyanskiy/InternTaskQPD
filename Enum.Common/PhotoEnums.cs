namespace Enum.Common;

/// <summary> Приоритетное хранилище для фото </summary>
public enum PhotoStorageType
{
    Database = 1,
    FileStorage = 2,
    Minio = 3,
    NotExists = 4
}

/// <summary> Метод получения фотографии </summary>
public enum PhotoMethod
{
    Empty = 0,
    Base64 = 1,
    // Моментальная ссылка на скачивание
    DirectLink = 2,
    // Асинхронная ссылка на скачивание
    Webhook = 3
}

/// <summary> Допустимые и используемые расширения фотографий </summary>
public enum PhotoFileExtension
{
    Jpg,
    Jpeg,
    Png,
    Heif,
    Webp,
    EmptyOrUnknown,
}

public static class PhotoFileExtensionHelper
{
    public static PhotoFileExtension MapExtension(string extension) =>
        extension.Trim().TrimStart('.').ToLowerInvariant() switch
        {
            "jpg" => PhotoFileExtension.Jpg,
            "jpeg" => PhotoFileExtension.Jpeg,
            "png" => PhotoFileExtension.Png,
            "heif" => PhotoFileExtension.Heif,
            "webp" => PhotoFileExtension.Webp,
            _ => PhotoFileExtension.EmptyOrUnknown
        };
}
