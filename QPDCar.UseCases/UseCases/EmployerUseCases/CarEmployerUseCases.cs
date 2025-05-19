using System.Net;
using System.Security.Claims;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ApplicationResult.Extensions;
using QPDCar.Models.ApplicationModels.ErrorTypes;
using QPDCar.Models.BusinessModels.CarModels;
using QPDCar.Models.BusinessModels.EmployerModels;
using QPDCar.Models.DtoModels.CarDtos;
using QPDCar.ServiceInterfaces;
using QPDCar.ServiceInterfaces.MailServices;
using QPDCar.UseCases.Helpers;
using QPDCar.UseCases.Models.CarModels;
using QPDCar.UseCases.Models.PhotoModels;
using QPDCar.UseCases.Models.UserModels;

namespace QPDCar.UseCases.UseCases.EmployerUseCases;

/// <summary> Действия с машиной для сотрудника </summary>
public class CarEmployerUseCases(ICarService carService, IMailSender mailSender)
{
    /// <summary> Кейс добавления сотрудником  новой машины в систему </summary>
    public async Task<ApplicationExecuteResult<CarUseCaseResponse>> NewCar(DtoForSaveCar carDto)
    {
        var warns = new List<ApplicationError>();
        // Сохраняем машину
        var carResult = await carService.CreateCarAsync(carDto);
        if (carResult.IsSuccess is false)
            return ApplicationExecuteResult<CarUseCaseResponse>.Failure().Merge(carResult);
        var car = carResult.Value!;
        
        // Нет фото - оправляем email
        if (carDto.Photo is null)
        {
            warns.Add(new ApplicationError(
                CarErrors.CarNotSetPhoto, "Нет фото",
                $"Машина {car.Id} создана без фото",
                ErrorSeverity.NotImportant));
            
            var body = $"Для машины {car.Id} не добавлено фото";
            var sendResult = await mailSender.SendAsync(car.Manager!.Email, "Машина установлена без фото", body);
            if (sendResult.IsSuccess is false)
                warns.Add(new ApplicationError(
                    EmailErrors.MailNotSend, "Email не отправлено",
                    "Письмо о том, что машина установлена без фото не отправлено",
                    ErrorSeverity.NotImportant));
        }

        var resp = CarHelper.BuildFullResponse(car);

        return ApplicationExecuteResult<CarUseCaseResponse>.Success(resp).WithWarnings(warns);
    }

    /// <summary> Кейс обновления сотрудником машины </summary>
    public async Task<ApplicationExecuteResult<CarUseCaseResponse>> UpdateCar(DtoForUpdateCar carDto, ClaimsPrincipal userClaims)
    {
        var warns = new List<ApplicationError>();
        var requestedEmployerId = Guid.Parse(userClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var requestedEmployerRoles = userClaims.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList()
            .Select(x => Enum.Parse<ApplicationRoles>(x, ignoreCase: true)).ToList();

        var carResult = await carService.ByIdAsync(carDto.CarId);
        if (carResult.IsSuccess is false)
            return ApplicationExecuteResult<CarUseCaseResponse>.Failure().Merge(carResult);
        var car = carResult.Value!;
        
        // Пользователь должен быть либо администратором, либо его id совпадать с Id менеджера машины
        if (!requestedEmployerRoles.Contains(ApplicationRoles.Admin) || requestedEmployerId != car.Manager!.Id)
            return ApplicationExecuteResult<CarUseCaseResponse>.Failure(new ApplicationError(
                UserErrors.DontEnoughPermissions, "Не достаточно прав",
                "Обновить машину может только менеджер за нее ответственный или администратор",
                ErrorSeverity.Critical, HttpStatusCode.Forbidden));

        if (carDto.NewManager is not null && !requestedEmployerRoles.Contains(ApplicationRoles.Admin))
        {
            carDto.NewManager = null;
            warns.Add(new ApplicationError(
                UserErrors.DontEnoughPermissions, "Менеджер не изменен",
                "Изменить ответственного менеджера может только администратор",
                ErrorSeverity.NotImportant));
        }
        
        var updatedResult = await carService.UpdateCarAsync(carDto);
        if (updatedResult.IsSuccess is false)
            return ApplicationExecuteResult<CarUseCaseResponse>.Failure().Merge(updatedResult);
        var updatedCar = carResult.Value!;
        
        var resp = CarHelper.BuildFullResponse(updatedCar);
        
        return ApplicationExecuteResult<CarUseCaseResponse>.Success(resp).WithWarnings(warns);
    }
    
    /// <summary> Кейс удаления сотрудником машины </summary>
    public async Task<ApplicationExecuteResult<Unit>> DeleteCar(int carId, ClaimsPrincipal userClaims)
    {
        var requestedEmployerId = Guid.Parse(userClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var requestedEmployerRoles = userClaims.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList()
            .Select(x => Enum.Parse<ApplicationRoles>(x, ignoreCase: true)).ToList();

        var carResult = await carService.ByIdAsync(carId);
        if (carResult.IsSuccess is false)
            return ApplicationExecuteResult<Unit>.Failure().Merge(carResult);
        var car = carResult.Value!;
        
        if (!requestedEmployerRoles.Contains(ApplicationRoles.Admin) || requestedEmployerId != car.Manager!.Id)
            return ApplicationExecuteResult<Unit>.Failure(new ApplicationError(
                UserErrors.DontEnoughPermissions, "Не достаточно прав",
                "Удалить машину может только менеджер за нее ответственный или администратор",
                ErrorSeverity.Critical, HttpStatusCode.Forbidden));
        
        var deletedResult = await carService.DeleteCarAsync(carId);
        if (deletedResult.IsSuccess is false)
            return ApplicationExecuteResult<Unit>.Failure().Merge(deletedResult);
        
        return ApplicationExecuteResult<Unit>.Success(Unit.Value);
    }

    /// <summary> Кейс получения сотрудником машины по id </summary>
    public async Task<ApplicationExecuteResult<CarUseCaseResponse>> GetCar(int carId, ClaimsPrincipal userClaims)
    {
        var requestedEmployerId = Guid.Parse(userClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var requestedEmployerRoles = userClaims.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList()
            .Select(x => Enum.Parse<ApplicationRoles>(x, ignoreCase: true)).ToList();

        var carResult = await carService.ByIdAsync(carId);
        if (carResult.IsSuccess is false)
            return ApplicationExecuteResult<CarUseCaseResponse>.Failure().Merge(carResult);
        var car = carResult.Value!;

        CarUseCaseResponse? resp = null;

        if (requestedEmployerRoles.Contains(ApplicationRoles.Admin) || requestedEmployerId == car.Manager!.Id)
            resp = CarHelper.BuildFullResponse(car);
        else 
            resp = CarHelper.BuildRestrictedResponse(car);
        
        return ApplicationExecuteResult<CarUseCaseResponse>.Success(resp);
    }

    /// <summary> Кейс получения сотрудником машин по параметрам </summary>
    public async Task<ApplicationExecuteResult<CarUseCaseResponsePage>> GetCars(DtoForSearchCars parameters, ClaimsPrincipal userClaims)
    {
        var warnings = new List<ApplicationError>();
        
        var requestedEmployerId = Guid.Parse(userClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var requestedEmployerRoles = userClaims.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList()
            .Select(x => Enum.Parse<ApplicationRoles>(x, ignoreCase: true)).ToList();
        
        var carsPageResult = await carService.ByParamsAsync(parameters);
        if (carsPageResult.IsSuccess is false)
            return ApplicationExecuteResult<CarUseCaseResponsePage>.Failure().Merge(carsPageResult);
        var carsPage = carsPageResult.Value!;

        var preparedCars = new List<CarUseCaseResponse>();
        
        var resp = new CarUseCaseResponsePage
        {
            TotalCount = carsPage.TotalCount,
            PageNumber = carsPage.PageNumber,
            PageSize = carsPage.PageSize,
        };

        foreach (var car in carsPage.Cars)
        {
            if (requestedEmployerRoles.Contains(ApplicationRoles.Admin) || requestedEmployerId == car.Manager!.Id) 
                preparedCars.Add(CarHelper.BuildFullResponse(car));
            else
            {
                warnings.Add(new ApplicationError(
                    UserErrors.DontEnoughPermissions, "Не полный ответ",
                    $"Частичный ответ: пользователь не является администратором или ответственным менеджером машины {car.Id}",
                    ErrorSeverity.NotImportant));
                preparedCars.Add(CarHelper.BuildRestrictedResponse(car));
            }
        }
        
        resp.Cars = preparedCars;
    
        return ApplicationExecuteResult<CarUseCaseResponsePage>.Success(resp)
            .WithWarnings(warnings)
            .WithWarnings(carsPageResult.GetWarnings);
    }
}