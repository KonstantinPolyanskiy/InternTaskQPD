using AutoMapper;
using Car.App.Models;
using CarService.Models;
using Contracts.Shared;

namespace CarService.Converters;

public class FormFileToApplicationPhotoConverter
    : ITypeConverter<IFormFile, ApplicationPhotoModel>
{
    /// <summary>
    /// Конвертирует <see cref="IFormFile"/> в <see cref="ApplicationPhotoModel"/>
    /// </summary>
    /// <param name="src"> IFormFile из ASP.NET Core</param>
    /// <param name="_"> Для совместимости</param>
    /// <param name="ctx"> Контекст операции</param>
    /// <returns><see cref="ApplicationPhotoModel"/> Фото для дальнейшей работы</returns>
    /// <exception cref="ArgumentNullException"><see cref="IFormFile"/> Источник фото null</exception>
    public ApplicationPhotoModel Convert(IFormFile src,
        ApplicationPhotoModel? _,
        ResolutionContext ctx)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));

        return new ApplicationPhotoModel()
        {
            Content       = src.OpenReadStream(),   
            FileExtension = Path.GetExtension(src.FileName),
            FileName      = src.FileName,
            Length        = src.Length
        };
    }
}

