using System.Net;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Private.ServicesInterfaces;
using Public.Models.ApplicationErrors;
using Public.Models.BusinessModels.CarModels;
using Public.Models.BusinessModels.PhotoModels;
using Public.Models.BusinessModels.StorageModels;
using Public.Models.BusinessModels.UserModels;
using Public.Models.CommonModels;
using Public.Models.DtoModels.CarDtoModels;
using Public.Models.DtoModels.PhotoDtoModels;
using Public.Models.Extensions;
using Public.UseCase.Helpers;
using Public.UseCase.Models.CarModels;
using Public.UseCase.Models.PhotoModels;
using Public.UseCase.Models.UserModels;

namespace Public.UseCase.UseCases.ManagerUseCases;

public class CarEmployerUseCase(ICarService carService, IUserService userService, IMailSenderService mailSender,
    IRoleService roleService, ILogger<CarEmployerUseCase> logger)
{
     /// <summary> Кейс добавления сотрудником (менеджером) новой машины в систему </summary>
     public async Task<ApplicationExecuteLogicResult<CarUseCaseResponse>> NewCar(DtoForAddCar carDto, ClaimsPrincipal employerClaims)
    {
        // Получаем менеджера
        var userId = Guid.Parse(employerClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // Сохраняем машину
        var saveCarResult = await PrepareAndSaveCar(userId, carDto);
        if (saveCarResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(saveCarResult);

        // Получаем снова с менеджером
        var carResult = await carService.CarById(saveCarResult.Value!.Id);
        if (carResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(carResult);
        var car = carResult.Value!;                           

        var resp = new CarUseCaseResponse
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
                Id = car.Manager!.Id,
                FirstName = car.Manager.FirstName,
                LastName = car.Manager.LastName,
                Email = car.Manager.Email,
                Login = car.Manager.Login,
            },
        };
        
        // Проверяем фото и сохраняем, если нет - отправка email и возврат
        if (carDto.Photo is null)
        {
            // TODO: пока фиксированно, но идейно тут должен быть ответственный сотрудник или сам менеджер
            var sendResult = await mailSender.SendAsync("admin@mail.ru", $"")
            var sendResult1 = await mailSender.SendNoPhotoNotifyEmailAsync("admin@mail.ru", car.Manager!.Login, car.Id);
            if (sendResult.IsSuccess is false)
                return ApplicationExecuteLogicResult<CarUseCaseResponse>.Success(resp)
                .WithWarning(new ApplicationError(EmailSendingErrors.EmailNotSend, "Письмо не отправлено",
                    "Машина без фото успешно добавлена, но оповещение об этом не отправлено", ErrorSeverity.NotImportant))
                .WithWarnings(carResult.GetWarnings);

            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Success(resp).WithWarnings(carResult.GetWarnings);
        }

        if (car.Photo is not null)
        {
            resp.Photo = new PhotoUseCaseResponse
            {
                MetadataId = car.Photo.Id,
                Extension = car.Photo.Extension,
                PhotoDataId = car.Photo.PhotoDataId,
                PhotoBytes = car.Photo.PhotoData
            };
        }
        
        // Возвращаем ответ
        return ApplicationExecuteLogicResult<CarUseCaseResponse>.Success(resp).WithWarnings(carResult.GetWarnings);
    }
    
    /// <summary> Кейс получения сотрудником машины по запросу </summary>
    public async Task<ApplicationExecuteLogicResult<CarsUseCaseResponse>> CarByParams(DtoForSearchCars searchParams, ClaimsPrincipal employerClaims)
    {

        var warnings = new List<ApplicationError>();
        
        // Получаем пользователя
        var requestedEmployerId = Guid.Parse(employerClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var userResult = await userService.ByLoginOrIdAsync(requestedEmployerId.ToString());
        if (userResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarsUseCaseResponse>.Failure().Merge(userResult);
        var requestedEmployer = userResult.Value!;
        
        var rolesResult = await roleService.GetRolesByUser(requestedEmployer);
        if (rolesResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarsUseCaseResponse>.Failure().Merge(rolesResult);
        var roles = rolesResult.Value!;
        
        var isAdmin   = roles.Contains(ApplicationUserRole.Admin);
        var isManager = roles.Contains(ApplicationUserRole.Manager);
        
        // Получаем машины
        var carsPageResult = await carService.CarsByParams(searchParams);
        if (carsPageResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarsUseCaseResponse>.Failure().Merge(carsPageResult);
        var carsPage = carsPageResult.Value!;
        
        var preparedCars = new List<CarUseCaseResponse>();
        var response = new CarsUseCaseResponse
        {
            PageSize = carsPage.PageSize,
            PageNumber = carsPage.PageNumber,
        };
        
        foreach (var domainCar in carsPage.DomainCars)
        {
            // Определяем доступ
            var isOwnManager = isManager && Guid.Parse(requestedEmployer.Id) == domainCar.Manager!.Id;
            var fullAccess = isAdmin || isOwnManager;
            
            if (!fullAccess)
                warnings.Add(new ApplicationError(RoleErrors.DontHaveEnoughPermissions, "Не полный ответ",
                    $"Частичный ответ: пользователь не является администратором или ответственным менеджером машины {domainCar.Id}",
                    ErrorSeverity.NotImportant));
            
            preparedCars.Add(fullAccess
                ? CarHelper.BuildFullResponse(domainCar)
                : CarHelper.BuildRestrictedResponse(domainCar));
        }
        
        response.Cars = preparedCars.ToArray();
        
        return ApplicationExecuteLogicResult<CarsUseCaseResponse>.Success(response)
            .WithWarnings(warnings)
            .WithWarnings(carsPageResult.GetWarnings);
    }
    
    /// <summary> Кейс получения сотрудником машины по ее Id </summary>
    public async Task<ApplicationExecuteLogicResult<CarUseCaseResponse>> CarById(int carId, ClaimsPrincipal employerClaims)
    {
        logger.LogInformation("Попытка получить данные о машине {id} сотрудником ", carId);

        // Получаем пользователя
        var requestedEmployerId = Guid.Parse(employerClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var userResult = await userService.ByLoginOrIdAsync(requestedEmployerId.ToString());
        if (userResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(userResult);
        var requestedEmployer = userResult.Value!;
        
        var rolesResult = await roleService.GetRolesByUser(requestedEmployer);
        if (rolesResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(rolesResult);
        var roles = rolesResult.Value!;
        
        // Получаем машину
        var carResult = await carService.CarById(carId);
        if (carResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(carResult);
        var car = carResult.Value!;
        
        return PrepareCarResponseForRoles(car, requestedEmployerId, roles, car.Manager!, car.Photo).WithWarnings(carResult.GetWarnings);
    }
    
    /// <summary> Кейс обновления сотрудником машины по ее Id </summary>
    public async Task<ApplicationExecuteLogicResult<CarUseCaseResponse>> UpdateCar(DtoForUpdateCar carDto, ClaimsPrincipal employerClaims)
    {
        logger.LogInformation("Попытка удалить данные о машине {id} сотрудником ", carDto.CarId);
        
        // Получаем сотрудника
        var requestedEmployerId = Guid.Parse(employerClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var userResult = await userService.ByLoginOrIdAsync(requestedEmployerId.ToString());
        if (userResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(userResult);
        var requestedEmployer = userResult.Value!;
        
        var rolesResult = await roleService.GetRolesByUser(requestedEmployer);
        if (rolesResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(rolesResult);
        var roles = rolesResult.Value!;
        
        var isAdmin   = roles.Contains(ApplicationUserRole.Admin);
        var isManager = roles.Contains(ApplicationUserRole.Manager);
        
        // Получаем машину
        var carResult = await carService.CarById(carDto.CarId);
        if (carResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(carResult);
        var car = carResult.Value!;
        
        // Определяем доступ
        var isOwnManager = isManager && Guid.Parse(requestedEmployer.Id) == car.Manager!.Id;
        var fullAccess = isAdmin || isOwnManager;
    
        // Если не админ - не может изменить ответственного за машину менеджера
        if (!isAdmin && carDto.NewManager is not null)
            carDto.NewManager = null;
        
        // Обновляем машину
        if (!fullAccess)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure(new ApplicationError(RoleErrors.DontHaveEnoughPermissions, "Машина не обновлена",
                "Недостаточно прав для обновления машины", ErrorSeverity.Critical, HttpStatusCode.Forbidden));
        
        var updatedCarResult = await carService.UpdateCarAsync(carDto);
        if (updatedCarResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(updatedCarResult);
        var updatedCar = updatedCarResult.Value!;
        
        return ApplicationExecuteLogicResult<CarUseCaseResponse>.Success(CarHelper.BuildFullResponse(updatedCar)).WithWarnings(updatedCarResult.GetWarnings);
    }
    
    /// <summary> Кейс удаления сотрудником машины </summary>
    public async Task<ApplicationExecuteLogicResult<Unit>> DeleteCar(int carId, ClaimsPrincipal employerClaims)
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
                "Удалить машину может только мендежер за нее отвественный или администратор",
                ErrorSeverity.Critical, HttpStatusCode.Forbidden));
        
        var deletedResult = await carService.DeleteCarById(carId);
        if (deletedResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(deletedResult);
        
        return ApplicationExecuteLogicResult<Unit>.Success(new Unit());
    }
    
    
    private async Task<ApplicationExecuteLogicResult<DomainCar>> PrepareAndSaveCar(Guid managerId, DtoForAddCar dto)
    {
        var carResult = await carService.CreateCarAsync(new DtoForSaveCar
        {
            ResponsiveManager = managerId,
            Brand = dto.Brand,
            Color = dto.Color,
            Price = dto.Price,
            CurrentOwner = dto.CurrentOwner,
            Mileage = dto.Mileage,
            Photo = dto.Photo is not null ? new DtoForSavePhoto
            {
                Extension = Enum.Parse<ImageFileExtensions>(dto.Photo!.RawExtension, ignoreCase: true),
                PriorityStorageType = StorageTypes.Minio,
                PhotoData = dto.Photo!.Data,
            } : null
        });
        if (carResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<DomainCar>.Failure().Merge(carResult);
        
        return ApplicationExecuteLogicResult<DomainCar>.Success(carResult.Value!);
    }

    private ApplicationExecuteLogicResult<CarUseCaseResponse> PrepareCarResponseForRoles(DomainCar car, Guid requestedUserId, List<ApplicationUserRole> requestedUserRoles, DomainEmployer manager, DomainPhoto? photo = null)
    {
        var isAdmin   = requestedUserRoles.Contains(ApplicationUserRole.Admin);
        var isManager = requestedUserRoles.Contains(ApplicationUserRole.Manager);
        
        // Определяем доступ
        var isOwnManager = isManager && requestedUserId == manager.Id;
        var fullAccess = isAdmin || isOwnManager;
        
        return fullAccess ? 
            ApplicationExecuteLogicResult<CarUseCaseResponse>.Success(CarHelper.BuildFullResponse(car)) : 
            ApplicationExecuteLogicResult<CarUseCaseResponse>.Success(CarHelper.BuildRestrictedResponse(car))
                .WithWarning(new ApplicationError(RoleErrors.DontHaveEnoughPermissions, "Не полный ответ",
                    "Частичный ответ: пользователь не является администратором или ответственным менеджером машины",
                    ErrorSeverity.NotImportant));
    }
}