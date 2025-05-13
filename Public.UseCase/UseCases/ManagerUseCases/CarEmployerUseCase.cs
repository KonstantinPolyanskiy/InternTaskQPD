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

public class CarEmployerUseCase
{
    #region Fields
    
    private readonly ICarService _carService;
    private readonly IPhotoService _photoService;
    private readonly IEmployerService _employerService;
    private readonly IUserService _userService;
    private readonly IMailSenderService _mailService;
    private readonly IRoleService _roleService;
    
    private readonly ILogger<EmployerUseCases> _logger;
    
    #endregion
    
    #region Ctor
    
    // ReSharper disable once ConvertToPrimaryConstructor
    public CarEmployerUseCase(ICarService carService, IPhotoService photoService, IEmployerService employerService, 
        ILogger<EmployerUseCases> logger, IMailSenderService mailService, IUserService userService, IRoleService roleService)
    {
        _carService = carService;
        _photoService = photoService;
        _employerService = employerService;
        _mailService = mailService;
        _userService = userService;
        _roleService = roleService;
        
        _logger = logger;
    }
    
    #endregion

    #region Публичные методы

    /// <summary> Кейс добавления сотрудником (менеджером) новой машины в систему </summary>
    public async Task<ApplicationExecuteLogicResult<CarUseCaseResponse>> NewCar(DtoForAddCar carDto, ClaimsPrincipal employerClaims)
    {
        // Получаем менеджера
        var userId = Guid.Parse(employerClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var managerResult = await _employerService.ManagerByUserIdAsync(userId);
        if (!managerResult.IsSuccess)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(managerResult);
        var manager = managerResult.Value!;

        // Сохраняем машину
        var carResult = await PrepareAndSaveCar(userId, carDto);
        if (carResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(carResult);
        var car = carResult.Value!;

        // Проверяем фото и сохраняем, если нет - отправка email и возврат
        if (carDto.Photo is null)
        {
            // TODO: пока фиксированно, но идейно тут должен быть ответственный сотрудник или сам менеджер
            var sendResult = await _mailService.SendNoPhotoNotifyEmailAsync("admin@mail.ru", manager.Login, car.Id);
            if (sendResult.IsSuccess is not true)
                return ApplicationExecuteLogicResult<CarUseCaseResponse>
                    .Success(PrepareCarResponse(car, manager))
                    .WithWarning(new ApplicationError(EmailSendingErrors.EmailNotSend, "Письмо не отправлено",
                        "Машина без фото успешно добавлена, но оповещение об этом не отправлено", ErrorSeverity.NotImportant))
                    .WithWarning(new ApplicationError(CarErrors.CarAddedWithoutPhoto, "Не добавлено фото",
                        $"Машина {car.Id} успешно добавлена, но без фотографии", ErrorSeverity.NotImportant));

            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Success(PrepareCarResponse(car, manager));
        }
        
        // Сохраняем фото
        var photoResult = await PrepareAndSavePhoto(carDto.Photo.RawExtension, car.Id, carDto.Photo.Data);
        if (photoResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(photoResult);
        var photo = photoResult.Value!;
        
        // Прикрепляем фото
        var setPhotoResult = await _carService.SetPhotoToCarAsync(car, photo);
        if (setPhotoResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(setPhotoResult);
        
        // Возвращаем ответ
        return ApplicationExecuteLogicResult<CarUseCaseResponse>.Success(PrepareCarResponse(car, manager, photo));
    }
    
    /// <summary> Кейс получения сотрудником машины по ее Id </summary>
    public async Task<ApplicationExecuteLogicResult<CarUseCaseResponse>> ById(int carId, ClaimsPrincipal employerClaims)
    {
        _logger.LogInformation("Попытка получить данные о машине {id} сотрудником ", carId);

        var warnings = new List<ApplicationError>();
        
        // Получаем пользователя
        var requestedEmployerId = Guid.Parse(employerClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var userResult = await _userService.UserByLoginOrIdAsync(requestedEmployerId.ToString());
        if (userResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(userResult);
        var requestedEmployer = userResult.Value!;
        
        var rolesResult = await _roleService.GetRolesByUser(requestedEmployer);
        if (rolesResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(rolesResult);
        var roles = rolesResult.Value!;
        
        // Получаем машину
        var carResult = await _carService.GetCarByIdAsync(carId);
        if (carResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(carResult);
        var car = carResult.Value!;
        
        // Получаем ее менеджера
        var managerResult = await _employerService.ManagerByUserIdAsync(car.Manager!.Id);
        if (managerResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<CarUseCaseResponse>.Failure().Merge(managerResult);
        var manager = managerResult.Value!;
        
        // Получаем фото машины
        var photoResult = await _photoService.GetPhotoByCarIdAsync(car.Id);
        if (photoResult.IsSuccess is not true)
            warnings.Add(new ApplicationError(
                CarErrors.CarNotFoundPhoto, "Фото не найдено",
                $"Для машины {car.Id} не найдено фото", ErrorSeverity.NotImportant));
        var photo = photoResult.Value!;
        
        
        return PrepareCarResponseForRoles(car, requestedEmployerId, roles, manager, photo).WithWarnings(warnings);
    }

    #endregion

    #region Служебные методы

    private async Task<ApplicationExecuteLogicResult<DomainCar>> PrepareAndSaveCar(Guid managerId, DtoForAddCar dto)
    {
        var carResult = await _carService.CreateCarAsync(new DtoForSaveCar
        {
            ResponsiveManager = managerId,
            Brand = dto.Brand,
            Color = dto.Color,
            Price = dto.Price,
            CurrentOwner = dto.CurrentOwner,
            Mileage = dto.Mileage,
        });
        if (carResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<DomainCar>.Failure().Merge(carResult);
        
        return ApplicationExecuteLogicResult<DomainCar>.Success(carResult.Value!);
    }

    private async Task<ApplicationExecuteLogicResult<DomainPhoto>> PrepareAndSavePhoto(string ext, int carId, byte[] data)
    {
        var photoResult = await _photoService.CreatePhotoAsync(new DtoForSavePhoto
        {
            Extension = Enum.Parse<ImageFileExtensions>(ext),
            PriorityStorageType = StorageTypes.Database,
            PhotoData = data,
            CarId = carId,
        });
        if (photoResult.IsSuccess is not true)
            return ApplicationExecuteLogicResult<DomainPhoto>.Failure().Merge(photoResult);
        
        return ApplicationExecuteLogicResult<DomainPhoto>.Success(photoResult.Value!);
    }

    private CarUseCaseResponse PrepareCarResponse(DomainCar car, DomainEmployer? manager = null, DomainPhoto? photo = null)
    {
        return new CarUseCaseResponse
        {
            Id = car.Id,
            Brand = car.Brand!,
            Color = car.Color!,
            Price = (decimal)car.Price!,
            CurrentOwner = car.CurrentOwner,
            Mileage = car.Mileage,
            CarCondition = car.CarCondition,
            PrioritySale = car.PrioritySale,

            Employer = manager is not null
                ? new EmployerUseCaseResponse
                {
                    Id = manager.Id,
                    FirstName = manager.FirstName,
                    LastName = manager.LastName,
                    Email = manager.Email,
                    Login = manager.Login,
                }
                : null,

            Photo = photo is not null
                ? new PhotoUseCaseResponse
                {
                    MetadataId = photo.Id,
                    Extension = photo.Extension,
                    PhotoDataId = photo.PhotoDataId,
                    PhotoBytes = photo.PhotoData
                }
                : null,
        };
    }

    private ApplicationExecuteLogicResult<CarUseCaseResponse> PrepareCarResponseForRoles(DomainCar car, Guid requestedUserId, List<ApplicationUserRole> requestedUserRoles, DomainEmployer manager, DomainPhoto? photo = null)
    {
        var isAdmin   = requestedUserRoles.Contains(ApplicationUserRole.Admin);
        var isManager = requestedUserRoles.Contains(ApplicationUserRole.Manager);
        
        // Определяем доступ
        var isOwnManager = isManager && requestedUserId == manager.Id;
        var fullAccess = isAdmin || isOwnManager;
        
        return fullAccess ? 
            ApplicationExecuteLogicResult<CarUseCaseResponse>.Success(CarHelper.BuildFullResponse(car, manager, photo)) : 
            ApplicationExecuteLogicResult<CarUseCaseResponse>.Success(CarHelper.BuildRestrictedResponse(car, manager, photo))
                .WithWarning(new ApplicationError(RoleErrors.DontHaveEnoughPermissions, "Не полный ответ",
                    "Частичный ответ: пользователь не является администратором или ответственным менеджером машины",
                    ErrorSeverity.NotImportant));
    }

    #endregion
}