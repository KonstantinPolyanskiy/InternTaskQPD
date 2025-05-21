using System.Net;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ApplicationResult.Extensions;
using QPDCar.Models.ApplicationModels.AuthModels;
using QPDCar.Models.ApplicationModels.ErrorTypes;
using QPDCar.ServiceInterfaces.MailServices;
using QPDCar.ServiceInterfaces.Publishers;
using QPDCar.ServiceInterfaces.UserServices;
using QPDCar.UseCases.Helpers;

namespace QPDCar.UseCases.UseCases.UserUseCases;

/// <summary> Кейсы для пользователей в целом </summary>
public class UserUseCases(IAuthService authService, IUserService userService, INotificationPublisher publisher, 
    IMailConfirmationService mailConfirmation, ILogger<UserUseCases> logger)  
{
    /// <summary> Вход пользователя </summary>
    public async Task<ApplicationExecuteResult<AuthTokensPair>> Login(string login, string password)
    {
        logger.LogInformation("Вход в аккаунт {login}", login);

        var warns = new List<ApplicationError>();
        
        // Авторизируем пользователя
        var authResult = await authService.SignInAndGetAuthTokensAsync(login, password);
        if (authResult.IsSuccess is false)
            return ApplicationExecuteResult<AuthTokensPair>.Failure().Merge(authResult);
        var pair = authResult.Value!;
        
        var now = DateTime.UtcNow;
        
        // Получаем пользователя для отправки email
        var userResult = await userService.ByLoginOrIdAsync(login);
        if (userResult.IsSuccess is false)
            warns.Add(UserErrorHelper.ErrorUserNotFound(login));
        var user = userResult.Value!;
        
        // Отправляем email о входе
        var sendResult = await publisher.NotifyAsync(EmailNotificationEventHelper
            .BuildAccountLoginEvent(user.Email!, string.Join(" ", now.ToShortDateString(), now.ToShortTimeString()), user.UserName!) );
        if (sendResult.IsSuccess is false)
            warns.Add(EmailErrorHelper.ErrorMailNotSendWarn(user.Email!, "вход в аккаунт"));
        
        return ApplicationExecuteResult<AuthTokensPair>.Success(pair)
            .WithWarnings(warns);
    }

    /// <summary> Выход пользователя </summary>
    public async Task<ApplicationExecuteResult<Unit>> Logout(ClaimsPrincipal claims, bool globally = false)
    {
        var loginClaim = claims.FindFirst(ClaimTypes.Name);
        if (loginClaim is null)
            return ApplicationExecuteResult<Unit>.Failure(new ApplicationError(
                UserErrors.LoginClaimNotFound, "JwtToken не содержит Name", 
                "Из claims не удалось получить Login", ErrorSeverity.Critical, HttpStatusCode.Forbidden));
        var login = loginClaim.ToString();
        
        var userResult = await userService.ByLoginOrIdAsync(login);
        if (userResult.IsSuccess is false)
            return ApplicationExecuteResult<Unit>.Failure().Merge(userResult);
        var user = userResult.Value!;
        
        var logoutResult = await authService.LogoutUserAsync(user, globally);
        if (logoutResult.IsSuccess is false)
            return ApplicationExecuteResult<Unit>.Failure().Merge(logoutResult);
        
        return ApplicationExecuteResult<Unit>.Success(Unit.Value);
    }

    /// <summary> Обновление сессии пользователя </summary>
    public async Task<ApplicationExecuteResult<AuthTokensPair>> Refresh(string refreshToken) 
    {
        var newPairResult = await authService.GetFreshTokensAsync(refreshToken);
        if (newPairResult.IsSuccess is false)
            return ApplicationExecuteResult<AuthTokensPair>.Failure().Merge(newPairResult);
        var pair = newPairResult.Value!;

        return ApplicationExecuteResult<AuthTokensPair>.Success(pair);
    }

    /// <summary> Подтверждение почты </summary>
    public async Task<ApplicationExecuteResult<Unit>> EmailConfirm(Guid userId, string confirmToken)
    {
        logger.LogInformation("Попытка подтверждения почты аккаунта с Id {id}", userId);
        
        var warns = new List<ApplicationError>();
        
        // Находим пользователя подтверждающего почту
        var userResult = await userService.ByLoginOrIdAsync(userId.ToString());
        if (userResult.IsSuccess is false)
            return ApplicationExecuteResult<Unit>.Failure().Merge(userResult);
        var user = userResult.Value!;
        
        // Подтверждаем почту
        var confirmResult = await mailConfirmation.ConfirmEmailAsync(user, confirmToken);
        if (confirmResult.IsSuccess is false)
            return ApplicationExecuteResult<Unit>.Failure().Merge(confirmResult);
        
        // Отправляем email об успешном подтверждении
        var thanksResult = await publisher.NotifyAsync(EmailNotificationEventHelper.BuildThanksEmailConfirmEvent(user.Email!, user.UserName!));
        if (thanksResult.IsSuccess is false)
            warns.Add(EmailErrorHelper.ErrorMailNotSendWarn(user.Email!, "подтверждение почты")); 
        
        return ApplicationExecuteResult<Unit>.Success(Unit.Value).WithWarnings(warns);
    }
}