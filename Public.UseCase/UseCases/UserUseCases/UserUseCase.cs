using System.Net;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Private.ServicesInterfaces;
using Public.Models.ApplicationErrors;
using Public.Models.BusinessModels.TokenModels;
using Public.Models.CommonModels;
using Public.Models.Extensions;
using Public.UseCase.Models;

namespace Public.UseCase.UseCases.UserUseCases;

public class UserUseCase(IAuthService authService, IEmailConfirmationService emailConfirmService,
    IUserService userService, ILogger<UserUseCase> logger, IMailSenderService mailSenderService)
{
    /// <summary> Процесс подтверждения почты клиентом </summary>
    public async Task<ApplicationExecuteLogicResult<UserEmailConfirmationResponse>> ConfirmEmailAsync(Guid userId, string confirmToken)
    {
        logger.LogInformation("Попытка подтверждения почты аккаунта с Id {id}", userId);

        var warnings = new List<ApplicationError>();
        
        var userResult = await userService.ByLoginOrIdAsync(userId.ToString());
        if (userResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<UserEmailConfirmationResponse>.Failure().Merge(userResult);
        var user = userResult.Value!;

        var confirmedResult = await emailConfirmService.ConfirmEmailAsync(user, confirmToken);
        if (confirmedResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<UserEmailConfirmationResponse>.Failure().Merge(confirmedResult);

        // Отправляем на почту сообщение о подтверждении
        var thanksMessage = Helper.ThanksForConfirmingEmailMessage(user);
        
        var thanksEmail = await mailSenderService.SendThanksForConfirmationEmailAsync(user.Email!, thanksMessage);
        if (thanksEmail.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserEmailConfirmationResponse>.Failure().Merge(thanksEmail);
        warnings.AddRange(thanksEmail.GetWarnings);
        
        logger.LogInformation("Успешное подтверждение почты пользователя с логином {login}", user.UserName);
        
        var confirmationResponse = new UserEmailConfirmationResponse
        {
            Login = user.UserName!,
            Email = user.Email!,
            Message = thanksMessage,
        };

        return ApplicationExecuteLogicResult<UserEmailConfirmationResponse>.Success(confirmationResponse).WithWarnings(warnings);
    }

    /// <summary> Процесс входа пользователя по логину и паролю </summary>
    public async Task<ApplicationExecuteLogicResult<AuthTokensPair>> LoginUserAsync(string login, string password)
    {
        logger.LogInformation("Попытка входа в аккаунт {login}", login);
        
        var warnings = new List<ApplicationError>();
        
        // Авторизируем пользователя и получаем токены
        var tokensResult = await authService.SignInAndGetAuthTokensAsync(login, password);
        if (tokensResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure().Merge(tokensResult);
        var tokens = tokensResult.Value!;
        var now = DateTime.UtcNow;
        
        // Получаем токена для отправки email
        var userResult = await userService.ByLoginOrIdAsync(login);
        if (userResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure().Merge(userResult);
        var user = userResult.Value!;

        // Отправляем email
        var sending = await mailSenderService.SendAccountLoginEmailAsync(user.Email!, $"В аккаунт {user.UserName} вошли сегодня {now.ToShortDateString()} {now.ToShortTimeString()}");
        warnings.AddRange(sending.GetWarnings); // Критические ошибки игнорируем, фактически вход совершен корректно
        
        logger.LogInformation("Успешный вход в аккаунт {login}", user.UserName);
     
        return ApplicationExecuteLogicResult<AuthTokensPair>.Success(tokens).WithWarnings(warnings);
    }

    /// <summary> Процесс получения свежей auth пары по refresh токен </summary>
    public async Task<ApplicationExecuteLogicResult<AuthTokensPair>> RefreshAuthPairAsync(string refreshToken)
    {
        var newTokensResult = await authService.GetFreshTokensAsync(refreshToken);
        if (newTokensResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure().Merge(newTokensResult);
        var tokens = newTokensResult.Value!;
        
        return ApplicationExecuteLogicResult<AuthTokensPair>.Success(tokens).WithWarnings(newTokensResult.GetWarnings);
    }

    /// <summary> Процесс выхода пользователя из аккаунта/всех аккаунтов </summary>
    public async Task<ApplicationExecuteLogicResult<Unit>> LogoutUserAsync(ClaimsPrincipal claims, bool globally)
    {
        logger.LogInformation("Попытка {try} выхода пользователя", globally ? "глобального" : "локального");
        
        var loginClaim = claims.FindFirst(ClaimTypes.Name);
        if (loginClaim is null)
            return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(
                JwtTokenErrors.UserIdNotFoundInClaims, "JwtToken не содержит Name", 
                "Из claims не удалось получить Login", ErrorSeverity.Critical, HttpStatusCode.Forbidden));
        
        var userResult = await userService.ByLoginOrIdAsync(loginClaim.Value);
        if (userResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(userResult);
        var user = userResult.Value!;
        
        var logoutResult = await authService.LogoutUserAsync(user, globally);
        if (logoutResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(logoutResult);
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value).WithWarnings(logoutResult.GetWarnings);
    }

}