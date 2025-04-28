namespace Car.App.Models.Photo;

/// <summary>Метод получения фотографии</summary>
public enum PhotoMethod
{
    Empty = 0,
    Base64 = 1,
    // Моментальная ссылка на скачивание
    DirectLink = 2,
    // Асинхронная ссылка на скачивание
    Webhook = 3
}
