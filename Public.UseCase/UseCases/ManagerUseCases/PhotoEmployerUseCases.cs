using System.Net;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Private.ServicesInterfaces;
using Public.Models.ApplicationErrors;
using Public.Models.BusinessModels.StorageModels;
using Public.Models.CommonModels;
using Public.Models.DtoModels.PhotoDtoModels;
using Public.Models.Extensions;
using Public.UseCase.Models.CarModels;
using Public.UseCase.Models.PhotoModels;
using Public.UseCase.Models.UserModels;

namespace Public.UseCase.UseCases.ManagerUseCases;

public class PhotoEmployerUseCases(ICarService carService, IUserService userService, IRoleService roleService)
{
    /// <summary> Кейс установка сотрудником фото машины </summary>
    public async Task<ApplicationExecuteLogicResult<CarUseCaseResponse>> AddCarPhoto(int carId, ClaimsPrincipal employerClaims, DtoForAddPhoto photoDto)
    {
        var userId = Guid.Parse(employerClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userResult = await userService.ByLoginOrIdAsync(userId.ToString());
        if (userResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(userResult);
        var user = userResult.Value!;

        var rolesResult = await roleService.GetRolesByUser(user);
        if (rolesResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(rolesResult);
        var roles = rolesResult.Value!;
        
        var carResult = await carService.CarById(carId);
        if (carResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(carResult);
        var car = carResult.Value!;
            
        // Добавить машину может только админ (для любой), или менеджер за нее отвественный
        if (car.Manager!.Id.ToString() != user.Id || !roles.Contains(ApplicationUserRole.Admin))
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure(new ApplicationError(
                RoleErrors.DontHaveEnoughPermissions, "Не достаточно прав",
                "Добавить машине фото может только мендежер за нее отвественный или администратор",
                ErrorSeverity.Critical, HttpStatusCode.Forbidden));

        var setPhotoResult = await carService.SetCarPhoto(new DtoForSavePhoto
        {
            Extension = Enum.Parse<ImageFileExtensions>(photoDto.RawExtension),
            PriorityStorageType = StorageTypes.Database,
            PhotoData = photoDto.Data,
            CarId = car.Id,
        });
        if (setPhotoResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(setPhotoResult);

        return ApplicationExecuteLogicResult<CarUseCaseResponse>.Success(new CarUseCaseResponse
        {
            Id = car.Id,
            Brand = car.Brand!,
            Color = car.Color!,
            Price = (decimal)car.Price!,
            CurrentOwner = car.CurrentOwner,
            Mileage = car.Mileage,
            CarCondition = car.CarCondition,
            PrioritySale = car.PrioritySale,
            Employer = new EmployerUseCaseResponse
            {
                Id = car.Manager.Id,
                FirstName = car.Manager.FirstName,
                LastName = car.Manager.LastName,
                Email = car.Manager.Email,
                Login = car.Manager.Login,
            },
            Photo = new PhotoUseCaseResponse
            {
                MetadataId = car.Photo!.Id,
                Extension = car.Photo.Extension,
                PhotoDataId = car.Photo.PhotoDataId,
                PhotoBytes = car.Photo.PhotoData
            }
        });
    }

    /// <summary> Кейс получения сотрудником фото машины </summary>
    public async Task<ApplicationExecuteLogicResult<PhotoUseCaseResponse>> GetCarPhoto(int photoId, ClaimsPrincipal employerClaims)
    {
        var photoResult = await carService.CarPhotoById(photoId);
        if (photoResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<PhotoUseCaseResponse>.Failure().Merge(photoResult);
        var photo = photoResult.Value!;

        return ApplicationExecuteLogicResult<PhotoUseCaseResponse>.Success(new PhotoUseCaseResponse
            {
                MetadataId = photo.Id,
                Extension = photo.Extension,
                PhotoDataId = photo.PhotoDataId,
                PhotoBytes = photo.PhotoData
            })
            .WithWarnings(photoResult.GetWarnings);
    }
    
    /// <summary> Кейс получения сотрудником фото машины </summary>
    public async Task<ApplicationExecuteLogicResult<Unit>> DeleteCarPhoto(int carId, ClaimsPrincipal employerClaims)
    {
        var userId = Guid.Parse(employerClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userResult = await userService.ByLoginOrIdAsync(userId.ToString());
        if (userResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(userResult);
        var user = userResult.Value!;

        var rolesResult = await roleService.GetRolesByUser(user);
        if (rolesResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(rolesResult);
        var roles = rolesResult.Value!;
        
        var carResult = await carService.CarById(carId);
        if (carResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(carResult);
        var car = carResult.Value!;
        
        if (car.Manager!.Id.ToString() != user.Id || !roles.Contains(ApplicationUserRole.Admin))
            return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(
                RoleErrors.DontHaveEnoughPermissions, "Не достаточно прав",
                "Удалить фото машины может только мендежер за нее отвественный или администратор",
                ErrorSeverity.Critical, HttpStatusCode.Forbidden));
        
        var deletedResult = await carService.DeleteCarPhoto(carId);
        if (deletedResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(deletedResult);
        
        return ApplicationExecuteLogicResult<Unit>.Success(new Unit());
    }
}