using AutoMapper;
using Microsoft.Extensions.Logging;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ApplicationResult.Extensions;
using QPDCar.Models.BusinessModels.EmployerModels;
using QPDCar.Models.BusinessModels.UserModels;
using QPDCar.Models.DtoModels.UserDtos;
using QPDCar.ServiceInterfaces.MailServices;
using QPDCar.ServiceInterfaces.Publishers;
using QPDCar.ServiceInterfaces.UserServices;
using QPDCar.UseCases.Helpers;

namespace QPDCar.UseCases.UseCases.ConsumerUseCases;

/// <summary> Кейсы с аккаунтом для клиента </summary>
public class ConsumerUseCases(IUserService userService, IRoleService roleService, IMailConfirmationService mailConfirmation, 
    INotificationPublisher publisher, IMapper mapper, ILogger<ConsumerUseCases> logger)
{
    /// <summary> Регистрация клиента </summary>
    public async  Task<ApplicationExecuteResult<UserSummary>> RegisterUser(DtoForCreateConsumer userData)
    {
        logger.LogInformation("Попытка регистрации пользователя с логином {login}", userData.Login);

        // Создаем аккаунт
        var userDto = mapper.Map<DtoForCreateUser>(userData);
        userDto.InitialRoles = [ApplicationRoles.Client];
        
        var userResult = await userService.CreateAsync(userDto);
        if (userResult.IsSuccess is false)
            return ApplicationExecuteResult<UserSummary>.Failure().Merge(userResult);
        var user = userResult.Value!;
        
        // Назначаем роли
        var rolesResult = await roleService.AddRolesToUser(user, [ApplicationRoles.Client]);
        if (rolesResult.IsSuccess is false)
            return ApplicationExecuteResult<UserSummary>.Failure().Merge(rolesResult);
        
        // Создаем токен и отправляем email
        var confirmTokenResult = await mailConfirmation.CreateConfirmationTokenAsync(user);
        if (confirmTokenResult.IsSuccess is false)
            return ApplicationExecuteResult<UserSummary>.Failure().Merge(confirmTokenResult);
        var token = confirmTokenResult.Value!;
        
        // Отправляем Email о подтверждении
        var url = new Uri($"/confirm-email?uid={ Uri.EscapeDataString(user.Id) }&code={ Uri.EscapeDataString(token) }", UriKind.Relative);
        var sendResult = await publisher.NotifyAsync(EmailNotificationEventHelper.BuildConfirmEmailEvent(user.Email!, url));
        if (sendResult.IsSuccess is false)
            return ApplicationExecuteResult<UserSummary>.Failure().Merge(sendResult);
        
        var resp = mapper.Map<UserSummary>(user);
        resp.Roles = [ApplicationRoles.Client];
        
        return ApplicationExecuteResult<UserSummary>.Success(resp);
    }
}