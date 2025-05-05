using Backend.Api.Controllers.ClientApi;
using Backend.App.Models.Business;
using Enum.Common;

namespace Backend.Api.Processors;

/// <summary>
/// Сервис для подготовки способа получения фото согласно правилам их хранения
/// </summary>
public class PhotoProcessor(LinkGenerator linkGenerator, IHttpContextAccessor httpAccessor)
{
    public IPhotoAccessor ProcessPhoto(Photo photo, PhotoMethod method)
    {
        return method switch
        {
            PhotoMethod.Empty => new EmptyAccessor(),
            PhotoMethod.Base64 => new Base64Getter(photo ?? throw new ArgumentNullException(nameof(photo))),
            PhotoMethod.DirectLink => new DownloadLinkGetter(photo, linkGenerator, httpAccessor),
            _ => throw new ApplicationException("cant process photo, unknown photo method")
        };
    }
    
}

public class EmptyAccessor : IPhotoAccessor
{
    public PhotoMethod AccessMethod => PhotoMethod.Empty;
    public string Access() => string.Empty;
}

public class Base64Getter(Photo photo) : IPhotoAccessor
{
    public PhotoMethod AccessMethod => PhotoMethod.Base64;
    public string Access() => Convert.ToBase64String(photo.Data.Data);
}

public class DownloadLinkGetter(Photo photo, LinkGenerator lg, IHttpContextAccessor ca) : IPhotoAccessor
{
    public PhotoMethod AccessMethod => PhotoMethod.DirectLink;
    public string Access()
    {
        ArgumentNullException.ThrowIfNull(ca);
        ArgumentNullException.ThrowIfNull(ca.HttpContext);
        ArgumentNullException.ThrowIfNull(lg);

        return lg.GetUriByRouteValues(
            httpContext: ca.HttpContext,
            routeName: "api/photo",
            values: new { photoId = photo.Id }
        )!;
    }
}