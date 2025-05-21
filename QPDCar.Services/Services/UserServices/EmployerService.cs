using System.Net;
using AutoMapper;
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

public class EmployerService(IUserService userService, IRoleService roleService, ICarRepository carRepo, 
    IMapper mapper, ILogger<EmployerService> logger) : IEmployerService
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
            return ApplicationExecuteResult<DomainEmployer>.Failure(UserErrorHelper.ErrorUserNotFoundWarning($"по машине {carId}"));
        var user = userResult.Value!;
        
        // Находим его роли
        var rolesResult = await roleService.GetRolesByUser(user);
        if (rolesResult.IsSuccess is false)
            return ApplicationExecuteResult<DomainEmployer>.Failure().Merge(rolesResult);
        var roles = rolesResult.Value!;
        
        var resp = mapper.Map<DomainEmployer>(user);
        resp.Roles = roles;
        
        // Возвращаем результат
        return ApplicationExecuteResult<DomainEmployer>.Success(resp);
    }
}