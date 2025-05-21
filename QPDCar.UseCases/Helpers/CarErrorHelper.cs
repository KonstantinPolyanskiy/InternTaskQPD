using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ErrorTypes;
using QPDCar.Models.BusinessModels.CarModels;
using QPDCar.UseCases.Models.CarModels;

namespace QPDCar.UseCases.Helpers;

internal static class CarErrorHelper
{
    internal static ApplicationError ErrorCarIsSoldWarning(int carId)
        => new(
            CarErrors.CarIsSold, "Запрошенная машина продана",
            $"Запрашиваемая машина {carId} уже продана",
            ErrorSeverity.NotImportant);
    
    internal static ApplicationError ErrorCarAddedWithoutPhotoWarning(int carId) 
        => new (
            CarErrors.CarNotSetPhoto, "Машина добавлена без фото",
            $"Машина {carId} добавлена без фотографии",
            ErrorSeverity.NotImportant);
    
    internal static ApplicationError ErrorRestrictedCarWarn(Guid userId, int carId)
        => new (
            UserErrors.DontEnoughPermissions, "Частичный ответ",
            $"Пользователь {userId} не имеет полного доступа к машине {carId}",
            ErrorSeverity.NotImportant);   
}