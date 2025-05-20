using Microsoft.Extensions.Logging;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ApplicationResult.Extensions;
using QPDCar.Models.ApplicationModels.Events;
using QPDCar.Models.BusinessModels.EmployerModels;
using QPDCar.Models.BusinessModels.UserModels;
using QPDCar.Models.DtoModels.UserDtos;
using QPDCar.ServiceInterfaces.MailServices;
using QPDCar.ServiceInterfaces.Publishers;
using QPDCar.ServiceInterfaces.UserServices;

namespace QPDCar.UseCases.UseCases.ConsumerUseCases;

/// <summary> Кейсы с аккаунтом для клиента </summary>
public class ConsumerUseCases(IUserService userService, IRoleService roleService, IMailConfirmationService mailConfirmation, 
    INotificationPublisher publisher, ILogger<ConsumerUseCases> logger)
{
    /// <summary> Регистрация клиента </summary>
    public async  Task<ApplicationExecuteResult<UserSummary>> RegisterUser(DtoForCreateConsumer userData)
    {
        logger.LogInformation("Попытка регистрации пользователя с логином {login}", userData.Login);

        var warns = new List<ApplicationError>();
        
        // Создаем аккаунт
        var userResult = await userService.CreateAsync(new DtoForCreateUser
        {
            FirstName = userData.FirstName,
            LastName = userData.LastName,
            Login = userData.Login,
            Email = userData.Email,
            Password = userData.Password,
            InitialRoles = [ApplicationRoles.Client],
        });
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
        
        var url = new Uri($"/confirm-email?uid={ Uri.EscapeDataString(user.Id) }&code={ Uri.EscapeDataString(token) }", UriKind.Relative);

        var body = $"Перейдите по {url} для подтверждения почты";
        
        var sendResult = await publisher.NotifyAsync(new EmailNotificationEvent
        {
            MessageId = Guid.NewGuid(),
            To = user.Email!,
            Subject = "Подтвердите почту",
            BodyHtml = body
        });
        if (sendResult.IsSuccess is false)
            return ApplicationExecuteResult<UserSummary>.Failure().Merge(sendResult);

        var resp = new UserSummary
        {
            Id = user.Id,
            Login = user.UserName!,
            FirstName = user.FirstName,
            LastName = user.LastName!,
            Email = user.Email!,
            EmailConfirmed = user.EmailConfirmed,
            Roles = [ApplicationRoles.Client],
        };
        
        return ApplicationExecuteResult<UserSummary>.Success(resp);
    }
}