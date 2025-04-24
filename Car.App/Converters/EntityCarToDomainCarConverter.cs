using AutoMapper;
using Car.App.Models;
using Contracts.Shared;
using Contracts.Types;

namespace Car.App.Converters;

/// <summary>
/// Конвертирует <see cref="Dal.Models.Car"/> в <see cref="ICar"/>  
/// </summary>
/// <param name="src"> Запись из Data Layer</param>
/// <param name="_"> Для совместимости </param>
/// <param name="ctx"> Контекст операции </param>
/// <returns><see cref="ICar"/> Сущность бизнес логики</returns>
/// <exception cref="NullReferenceException"><see cref="UsedCar"/> имеет недопустимое поле</exception>
/// <exception cref="InvalidDataException"><see cref="UsedCar"/> имеет недопустимое поле</exception>
public class EntityCarToDomainCarConverter : ITypeConverter<Dal.Models.Car, ICar>
{
    public ICar Convert(Dal.Models.Car src, ICar? _, ResolutionContext ctx)
    {
        // TODO: подумать над исключениями
        
        var type = (CarTypes)src.CarType;

        return type switch
        {
            CarTypes.NewCar => new NewCar
            {
                Id    = src.Id,
                Brand = src.Brand,
                Color = src.Color,
                Price = src.Price,
                Photo = ctx.Mapper.Map<ApplicationPhotoModel?>(src.Photo)
            },

            CarTypes.UsedCar => new UsedCar
            {
                Id           = src.Id,
                Brand        = src.Brand,
                Color        = src.Color,
                Price        = src.Price,
                Photo        = ctx.Mapper.Map<ApplicationPhotoModel?>(src.Photo),
                Mileage      = (int)src.Mileage!,
                CurrentOwner = src.CurrentOwner!
            },

            _ => throw new ArgumentOutOfRangeException(
                nameof(src.CarType), src.CarType, "unknown car type")
        };
    }
}