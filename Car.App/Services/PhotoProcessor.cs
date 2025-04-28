using Car.App.Models.Photo;
using Car.App.Models.PhotoModels;

namespace Car.App.Services;

/// <summary>
/// Сервис для подготовки способа получения фото согласно правилам их хранения
/// </summary>
public class PhotoProcessor(string fsStoragePath = "")
{
    public IPhotoAccessMethod ProcessPhoto(ICarPhoto photo, PhotoMethod method, string? carTermId = null)
    {
        return method switch
        {
            PhotoMethod.Empty => new EmptyGetter(),
            PhotoMethod.Base64 => new Base64Getter(photo ?? throw new ArgumentNullException(nameof(photo))),
            PhotoMethod.DirectLink => new DownloadLinkGetter(photo, carTermId! , fsStoragePath),
            _ => throw new ApplicationException("cant process photo, unknown photo method")
        };
    }
    
}

public class EmptyGetter : IPhotoAccessMethod
{
    private static string PrepareEmpty() => string.Empty;
    
    public PhotoMethod Method => PhotoMethod.Empty;
    public string Value { get; set; } = PrepareEmpty();
}

public class Base64Getter(ICarPhoto photo) : IPhotoAccessMethod
{
    private static string PrepareBase64(ICarPhoto photo)
    {
        var original = photo.Data?.Content
                       ?? throw new ApplicationException("Photo Data.Content is null");

        if (original.CanSeek)
            original.Position = 0;

        using var ms = new MemoryStream();
        original.CopyTo(ms);

        var bytes = ms.ToArray();
        return Convert.ToBase64String(bytes);
    }

    public PhotoMethod Method => PhotoMethod.Base64;
    public string Value { get; set; } = PrepareBase64(photo);
}

public class DownloadLinkGetter(ICarPhoto photo, string carTermId = "0", string fsStoragePath = "") : IPhotoAccessMethod
{
    private static string PrepareDownloadLink(ICarPhoto photo, string carTermId, string fsStoragePath = "")
    {
        if (int.TryParse(carTermId, out var carId) is false)
            throw new ArgumentException("Search term must be a number for database photo storage");
        
        // БД — есть Id и стрим
        if (photo.Id is not null && photo.Data?.Content != null)
        {
            return $"/car/carimage/{photo.Id}";
        }

        // MinIO — есть имя файла и стрим, но нет id
        if (!string.IsNullOrWhiteSpace(photo.PhotoName) && photo.Data?.Content != null && photo.Id is null or "0")
            return $"/car/carimage/{photo.PhotoName}";

        // FS - есть ид машины, но нет id и названия фото
        if (!string.IsNullOrWhiteSpace(fsStoragePath) && carId != 0)
        {
            var fullPath = Path.GetFullPath(fsStoragePath);
            if (Directory.Exists(fullPath))
            {
                var matchingFile = Directory.EnumerateFiles(fullPath, $"{carId}-*")
                    .Select(Path.GetFileName)
                    .FirstOrDefault();

                if (matchingFile != null)
                    return $"/car/carimage/{matchingFile}";
            }
        }

        return "cannot get download link";
    }

    public PhotoMethod Method => PhotoMethod.DirectLink;
    public string Value { get; set; } = PrepareDownloadLink(photo, carTermId, fsStoragePath);
}