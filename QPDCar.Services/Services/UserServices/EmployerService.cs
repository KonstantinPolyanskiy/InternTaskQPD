using System.Net;
using Microsoft.Extensions.Logging;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ApplicationResult.Extensions;
using QPDCar.Models.ApplicationModels.ErrorTypes;
using QPDCar.Models.BusinessModels.CarModels;
using QPDCar.Models.BusinessModels.EmployerModels;
using QPDCar.Models.StorageModels;
using QPDCar.ServiceInterfaces;
using QPDCar.ServiceInterfaces.UserServices;
using QPDCar.Services.ErrorHelpers;
using QPDCar.Services.Repositories;

namespace QPDCar.Services.Services.UserServices;

public class EmployerService(IUserService userService, IRoleService roleService, ICarRepository carRepo, ILogger<EmployerService> logger) : IEmployerService
{
    private const string CarObjectName = "Car";
    public async Task<ApplicationExecuteResult<DomainEmployer>> ManagerByCarId(int carId)
    {
        logger.LogInformation("Получение менеджера машины {carId}", carId);
        
        // Находим машину с этим менеджером
        var carResult = await carRepo.ByIdAsync(carId);
        if (carResult.IsSuccess is false)
            return DbErrorHelper.WrapAllDbErrors<CarEntity, DomainEmployer>(CarErrors.CarNotFound, carResult, string.Join(" ", CarObjectName, carId.ToString()));
        var car = carResult.Value!;
        
        // Находим аккаунт менеджера
        var userResult = await userService.ByLoginOrIdAsync(car.ResponsiveManagerId.ToString());
        if (userResult.IsSuccess is false)
            return ApplicationExecuteResult<DomainEmployer>.Failure(new ApplicationError(
                UserErrors.UserNotFound, "Пользователь не найден",
                $"Менеджер для машины с id {car.Id} не найден",
                ErrorSeverity.Critical, HttpStatusCode.NotFound));
        var user = userResult.Value!;
        
        // Находим его роли
        var rolesResult = await roleService.GetRolesByUser(user);
        if (rolesResult.IsSuccess is false)
            return ApplicationExecuteResult<DomainEmployer>.Failure().Merge(rolesResult);
        var roles = rolesResult.Value!;
        
        // Возвращаем результат
        return ApplicationExecuteResult<DomainEmployer>.Success(new DomainEmployer
        {
            Id = Guid.Parse(user.Id),
            FirstName = user.FirstName,
            LastName = user.LastName!,
            Email = user.Email!,
            Login = user.UserName!,
            Roles = roles
        });
    }
}