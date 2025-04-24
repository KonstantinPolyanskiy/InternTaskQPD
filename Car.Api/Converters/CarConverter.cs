using AutoMapper;
using Car.Api.Profiles.Models;
using Car.App.Models;
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

public class DomainCarToResponseCarConverter 
    : ITypeConverter<ICar, CarResponse>
{
    /// <summary>
    /// Конвертирует Domain object <see cref="ICar"/> в <see cref="CarResponse"/>
    /// </summary>
    /// <param name="src"> Общий интерфейс для машин <see cref="ICar"/> </param>
    /// <param name="_"> Для совместимости </param>
    /// <param name="ctx"> Контекст операции </param>
    public CarResponse Convert(ICar src, 
        CarResponse? _, ResolutionContext ctx)
    {
        var response = new CarResponse
        {
            Id = (int)src.Id!,
            CarType = nameof(src.CarType),
            
            StandardParameters = new StandardCarParameters
            {
                Brand = src.Brand,
                Color = src.Color,
                Price = src.Price,
            }
        };

        if (src is UsedCar u)
        {
            response.UsedParameters = new UsedCarParameters
            {
                Mileage = u.Mileage,
                CurrentOwner = u.CurrentOwner,
            };
        }

        return response;
    }
}