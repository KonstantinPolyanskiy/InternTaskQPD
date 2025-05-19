using System.Net;
using System.Security.Claims;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ApplicationResult.Extensions;
using QPDCar.Models.ApplicationModels.ErrorTypes;
using QPDCar.Models.BusinessModels.CarModels;
using QPDCar.Models.BusinessModels.EmployerModels;
using QPDCar.Models.BusinessModels.PhotoModels;
using QPDCar.Models.DtoModels.CarDtos;
using QPDCar.Models.DtoModels.PhotoDtos;
using QPDCar.ServiceInterfaces;
using QPDCar.UseCases.Helpers;
using QPDCar.UseCases.Models.CarModels;
using QPDCar.UseCases.Models.PhotoModels;

namespace QPDCar.UseCases.UseCases.EmployerUseCases;

/// <summary> Действия с фото машины для сотрудника </summary>
public class PhotoEmployerUseCases(ICarService carService)
{
    /// <summary> Добавление сотрудником фото машине </summary>
    public async Task<ApplicationExecuteResult<CarUseCaseResponse>> AddCarPhoto(int carId, ClaimsPrincipal userClaims, DtoForAddPhoto addingPhotoDto)
    {
        var warns = new List<ApplicationError>();
        
        var requestedEmployerId = Guid.Parse(userClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var requestedEmployerRoles = userClaims.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList()
            .Select(x => Enum.Parse<ApplicationRoles>(x, ignoreCase: true)).ToList();
        
        var carResult = await carService.ByIdAsync(carId);
        if (carResult.IsSuccess is false)
            return ApplicationExecuteResult<CarUseCaseResponse>.Failure().Merge(carResult);
        var car = carResult.Value!;
        
        if (car.Photo != null)
            warns.Add(new ApplicationError(
                CarErrors.CarAlredyHavePhoto, "Машина уже с фото",
                $"У машины {car.Id} уже есть фото {car.Photo.Id}",
                ErrorSeverity.NotImportant));
        
        if (!requestedEmployerRoles.Contains(ApplicationRoles.Admin) || requestedEmployerId != car.Manager!.Id)
            return ApplicationExecuteResult<CarUseCaseResponse>.Failure(new ApplicationError(
                UserErrors.DontEnoughPermissions, "Не достаточно прав",
                "Обновить фото машины может только менеджер за нее ответственный или администратор",
                ErrorSeverity.Critical, HttpStatusCode.Forbidden));
        
        var updatedCar = await carService.SetCarPhotoAsync(carId, new DtoForSavePhoto
        {
            Extension = Enum.Parse<ImageFileExtensions>(addingPhotoDto.RawExtension),
            PriorityStorageType = PhotoStorageTypes.Database,
            PhotoData = addingPhotoDto.Data
        });
        if (updatedCar.IsSuccess is false)
            return ApplicationExecuteResult<CarUseCaseResponse>.Failure().Merge(updatedCar);
        var photo = updatedCar.Value!;
        
        car.Photo = photo;

        var resp = CarHelper.BuildFullResponse(car);
        
        return ApplicationExecuteResult<CarUseCaseResponse>
            .Success(resp)
            .WithWarnings(warns);
    }
    
    /// <summary> Удаление сотрудником фото у машины </summary>
    public async Task<ApplicationExecuteResult<CarUseCaseResponse>> DeleteCarPhoto(int carId, ClaimsPrincipal userClaims)
    {
        var requestedEmployerId = Guid.Parse(userClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var requestedEmployerRoles = userClaims.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList()
            .Select(x => Enum.Parse<ApplicationRoles>(x, ignoreCase: true)).ToList();
        
        var carResult = await carService.ByIdAsync(carId);
        if (carResult.IsSuccess is false)
            return ApplicationExecuteResult<CarUseCaseResponse>.Failure().Merge(carResult);
        var car = carResult.Value!;
        
        if (!requestedEmployerRoles.Contains(ApplicationRoles.Admin) || requestedEmployerId != car.Manager!.Id)
            return ApplicationExecuteResult<CarUseCaseResponse>.Failure(new ApplicationError(
                UserErrors.DontEnoughPermissions, "Не достаточно прав",
                "Удалить фото машины может только менеджер за нее ответственный или администратор",
                ErrorSeverity.Critical, HttpStatusCode.Forbidden));

        var updatedCarResult = await carService.DeleteCarPhotoAsync(carId);
        if (updatedCarResult.IsSuccess is false)
            return ApplicationExecuteResult<CarUseCaseResponse>.Failure().Merge(updatedCarResult);
        var updatedCar = updatedCarResult.Value!;
        
        var resp = CarHelper.BuildFullResponse(car);

        return ApplicationExecuteResult<CarUseCaseResponse>.Success(resp);
    }
}