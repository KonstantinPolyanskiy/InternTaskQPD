using System.Net;
using System.Security.Claims;
using AutoMapper;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ApplicationResult.Extensions;
using QPDCar.Models.BusinessModels.EmployerModels;
using QPDCar.Models.DtoModels.CarDtos;
using QPDCar.ServiceInterfaces;
using QPDCar.ServiceInterfaces.Publishers;
using QPDCar.UseCases.Helpers;
using QPDCar.UseCases.Models.CarModels;

namespace QPDCar.UseCases.UseCases.EmployerUseCases;

/// <summary> Действия с машиной для сотрудника </summary>
public class CarEmployerUseCases(ICarService carService, IMapper mapper, INotificationPublisher publisher)
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
            warns.Add(CarErrorHelper.ErrorCarAddedWithoutPhotoWarning(car.Id));
            
            var sendResult = await publisher.NotifyAsync(EmailNotificationEventHelper.BuildCarWithoutPhotoEmailEvent(car.Manager!.Email, car.Id));
            if (sendResult.IsSuccess is false)
                warns.Add(EmailErrorHelper.ErrorMailNotSendWarn(car.Manager.Email, $"фото {car.Id} не добавлено"));
        }

        var resp = CarHelper.BuildFullResponse(car);

        return ApplicationExecuteResult<CarUseCaseResponse>.Success(resp).WithWarnings(warns);
    }

    /// <summary> Кейс обновления сотрудником машины </summary>
    public async Task<ApplicationExecuteResult<CarUseCaseResponse>> UpdateCar(DtoForUpdateCar carDto, ClaimsPrincipal userClaims)
    {
        var warns = new List<ApplicationError>();
        
        var requestedEmployerId = Guid.Parse(userClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var carResult = await carService.ByIdAsync(carDto.CarId);
        if (carResult.IsSuccess is false)
            return ApplicationExecuteResult<CarUseCaseResponse>.Failure().Merge(carResult);
        var car = carResult.Value!;
        
        // Пользователь должен быть либо администратором, либо его id совпадать с Id менеджера машины
        if (!EmployerRoles(userClaims).Contains(ApplicationRoles.Admin) || requestedEmployerId != car.Manager!.Id)
            return ApplicationExecuteResult<CarUseCaseResponse>
                .Failure(RoleErrorHelper
                    .ErrorDontEnoughPermissionWarning("изменить машину", car.Id.ToString())
                    .ToCritical(HttpStatusCode.Forbidden));

        if (carDto.NewManager is not null && !EmployerRoles(userClaims).Contains(ApplicationRoles.Admin))
        {
            carDto.NewManager = null;
            warns.Add(RoleErrorHelper.ErrorDontEnoughPermissionWarning("изменить менеджера машины", car.Id.ToString()));
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

        var carResult = await carService.ByIdAsync(carId);
        if (carResult.IsSuccess is false)
            return ApplicationExecuteResult<Unit>.Failure().Merge(carResult);
        var car = carResult.Value!;
        
        if (!EmployerRoles(userClaims).Contains(ApplicationRoles.Admin) || requestedEmployerId != car.Manager!.Id)
            return ApplicationExecuteResult<Unit>
                .Failure(CarErrorHelper.ErrorRestrictedCarWarn(requestedEmployerId, car.Id)
                .ToCritical(HttpStatusCode.Forbidden));
        
        var deletedResult = await carService.DeleteCarAsync(carId);
        if (deletedResult.IsSuccess is false)
            return ApplicationExecuteResult<Unit>.Failure().Merge(deletedResult);
        
        return ApplicationExecuteResult<Unit>.Success(Unit.Value);
    }

    /// <summary> Кейс получения сотрудником машины по id </summary>
    public async Task<ApplicationExecuteResult<CarUseCaseResponse>> GetCar(int carId, ClaimsPrincipal userClaims)
    {
        var requestedEmployerId = Guid.Parse(userClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var carResult = await carService.ByIdAsync(carId);
        if (carResult.IsSuccess is false)
            return ApplicationExecuteResult<CarUseCaseResponse>.Failure().Merge(carResult);
        var car = carResult.Value!;

        CarUseCaseResponse? resp;

        if (EmployerRoles(userClaims).Contains(ApplicationRoles.Admin) || requestedEmployerId == car.Manager!.Id)
            resp = CarHelper.BuildFullResponse(car);
        else 
            resp = CarHelper.BuildRestrictedResponse(car);
        
        return ApplicationExecuteResult<CarUseCaseResponse>.Success(resp);
    }

    /// <summary> Кейс получения сотрудником машин по параметрам </summary>
    public async Task<ApplicationExecuteResult<CarUseCaseResponsePage>> GetCars(DtoForSearchCars parameters, ClaimsPrincipal userClaims)
    {
        var warnings = new List<ApplicationError>();
        
        var employerId = Guid.Parse(userClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        // Получаем страницу с машинами
        var carsPageResult = await carService.ByParamsAsync(parameters);
        if (carsPageResult.IsSuccess is false)
            return ApplicationExecuteResult<CarUseCaseResponsePage>.Failure().Merge(carsPageResult);
        var carsPage = carsPageResult.Value!;

        var preparedCars = new List<CarUseCaseResponse>();
        
        // Строим ответ с данными согласно роли запросившего сотрудника
        foreach (var car in carsPage.Cars)
        {
            if (EmployerRoles(userClaims).Contains(ApplicationRoles.Admin) || employerId == car.Manager!.Id) 
                preparedCars.Add(CarHelper.BuildFullResponse(car));
            else
                warnings.Add(CarErrorHelper.ErrorRestrictedCarWarn(employerId, car.Id));
        }
        
        var resp = mapper.Map<CarUseCaseResponsePage>(carsPage);
        resp.Cars = preparedCars;
    
        return ApplicationExecuteResult<CarUseCaseResponsePage>.Success(resp)
            .WithWarnings(warnings)
            .WithWarnings(carsPageResult.GetWarnings);
    }

    private List<ApplicationRoles> EmployerRoles(ClaimsPrincipal userClaims)
    {
        return userClaims.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList()
            .Select(x => Enum.Parse<ApplicationRoles>(x, ignoreCase: true)).ToList();
    }
}