using Car.App.Models;
using Car.Dal.Models;
using Contracts.Shared;
using Contracts.Types;

namespace Car.App.Services;

/// <summary>
/// Сервис для подготовки способа получения фото согласно правилам их хранения
/// </summary>
public class PhotoProcessor(string fsStoragePath = "")
{
    public IPhotoGetter ProcessPhoto(ICarPhoto photo, PhotoMethod method, int carId = 0)
    {
        return method switch
        {
            PhotoMethod.Empty => new EmptyGetter(),
            PhotoMethod.Base64 => new Base64Getter(photo.Model ?? throw new ArgumentNullException(nameof(photo.Model))),
            PhotoMethod.Link => new DownloadLinkGetter(photo, carId, fsStoragePath),
            _ => throw new ApplicationException("cant process photo, unknown photo method")
        };
    }
    
}

public class EmptyGetter : IPhotoGetter
{
    public string WayToGet { get; set; } = string.Empty;
}

public class Base64Getter(ApplicationPhotoModel photo) : IPhotoGetter
{
    private static string PrepareBase64(ApplicationPhotoModel photo)
    {
        using (photo.Content)
        {
            if (photo.Content is MemoryStream ms && ms.TryGetBuffer(out var buffer))
                return Convert.ToBase64String(buffer);
            
            using var copy = new MemoryStream();
            
            if (photo.Content is null)
                throw new ApplicationException("cannot prepare photo to base64, content is null");
            
            photo.Content.CopyTo(copy);
            return Convert.ToBase64String(copy.GetBuffer(), 0, (int)copy.Length);
        }
    }

    public string WayToGet { get; set; } = PrepareBase64(photo);
}

public class DownloadLinkGetter(ICarPhoto photo, int carId = 0, string fsStoragePath = "") : IPhotoGetter
{
    private static string PrepareDownloadLink(ICarPhoto photo, int carId, string fsStoragePath = "")
    {
        // БД — есть Id и стрим
        if (photo.Id is not null && photo.Model?.Content != null)
        {
            return $"/car/carimage/{photo.Id}";
        }

        // MinIO — есть имя файла и стрим, но нет id
        if (!string.IsNullOrWhiteSpace(photo.PhotoName) && photo.Model?.Content != null && photo.Id is null or 0)
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

        throw new FileNotFoundException("сannot determine download link for photo");
    }
    
    
    public string WayToGet { get; set; } = PrepareDownloadLink(photo, carId);
}