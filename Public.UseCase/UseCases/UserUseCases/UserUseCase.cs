using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Private.ServicesInterfaces;
using Private.StorageModels;
using Public.Models.ApplicationErrors;
using Public.Models.BusinessModels.TokenModels;
using Public.Models.CommonModels;
using Public.Models.Extensions;
using Public.UseCase.Models;

namespace Public.UseCase.UseCases.UserUseCases;

public class UserUseCase
{
    private readonly ILogger<UserUseCase> _logger;
    
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly ITokenService _tokenService;
    private readonly IMailSenderService _mailSenderService;

    // ReSharper disable once ConvertToPrimaryConstructor
    public UserUseCase(IUserService userService, IRoleService roleService, ITokenService tokenService,
        IMailSenderService mailSenderService, 
        ILogger<UserUseCase> logger)
    {
        _userService = userService;
        _roleService = roleService;
        _tokenService = tokenService;
        _mailSenderService = mailSenderService;
        
        _logger = logger;
    }
    
    /// <summary> Процесс подтверждения почты клиентом </summary>
    public async Task<ApplicationExecuteLogicResult<UserEmailConfirmationResponse>> ConfirmEmailAsync(Guid userId, string confirmToken)
    {
        _logger.LogInformation("Попытка подтверждения почты аккаунта с Id {id}", userId);

        var warnings = new List<ApplicationError>();
        
        // Получаем пользователя, проверяем существует ли он вообще
        var user = await _userService.UserByLoginOrIdAsync(userId.ToString());
        if (user.IsSuccess is not true || user.Value is null)
            return ApplicationExecuteLogicResult<UserEmailConfirmationResponse>.Failure().Merge(user);
        warnings.AddRange(user.GetWarnings);
        
        // Проверяем что токен подтверждения валиден - не истек, userId совпадают и т.д.
        var confirmed = await _tokenService.CheckEmailConfirmationTokenAsync(userId, confirmToken);
        if (confirmed.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserEmailConfirmationResponse>.Failure().Merge(confirmed);
        warnings.AddRange(confirmed.GetWarnings);
        
        // Устанавливаем почту как подтвержденную
        var setConfirmed = await _userService.SetEmailAddressAsConfirmedAsync(user.Value);
        if (setConfirmed.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserEmailConfirmationResponse>.Failure().Merge(setConfirmed);
        warnings.AddRange(setConfirmed.GetWarnings);
        
        // Отправляем на почту сообщение о подтверждении
        var thanksMessage = Helper.ThanksForConfirmingEmailMessage(user.Value);
        
        var thanksEmail = await _mailSenderService.SendThanksForConfirmationEmailAsync(user.Value.Email!, thanksMessage);
        if (thanksEmail.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserEmailConfirmationResponse>.Failure().Merge(thanksEmail);
        warnings.AddRange(thanksEmail.GetWarnings);
        
        _logger.LogInformation("Успешное подтверждение почты пользователя с логином {login}", user.Value.UserName);
        
        var confirmationResponse = new UserEmailConfirmationResponse
        {
            Login = user.Value.UserName!,
            Email = user.Value.Email!,
            Message = Helper.ThanksForConfirmingEmailMessage(user.Value),
        };

        return ApplicationExecuteLogicResult<UserEmailConfirmationResponse>.Success(confirmationResponse).WithWarnings(warnings);
    }

    /// <summary> Процесс входа пользователя по логину и паролю </summary>
    public async Task<ApplicationExecuteLogicResult<AuthTokensPair>> LoginUserAsync(string login, string password)
    {
        _logger.LogInformation("Попытка входа в аккаунт {login}", login);
        
        var warnings = new List<ApplicationError>();
        
        // Находим пользователя
        var user = await _userService.UserByLoginOrIdAsync(login);
        if (user.IsSuccess is not true)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure().Merge(user);
        warnings.AddRange(user.GetWarnings);

        var roles = await _roleService.GetRolesByUser(user.Value!);
        if (user.IsSuccess is not true)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure().Merge(roles);
            
        // Проверяем валиден ли пароль
        var correctPassword = await _userService.CheckPasswordForUserAsync(user.Value!, password);
        if (correctPassword.IsSuccess is not true)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure().Merge(user);
        warnings.AddRange(user.GetWarnings);
        
        // Генерируем пару токенов
        var authPair = await _tokenService.GenerateAuthTokensPairAsync(user.Value!, roles.Value!, 15);
        if (authPair.IsSuccess is not true)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure().Merge(authPair);
        warnings.AddRange(user.GetWarnings);
        
        // Отправляем email о входе
        var loginMessage = Helper.AccountLoginEmailMessage(user.Value!);
        
        var sending = await _mailSenderService.SendAccountLoginEmailAsync(user.Value!.Email!, loginMessage);
        warnings.AddRange(sending.GetWarnings); // Критические ошибки игнорируем, фактически вход совершен корректно
        
        _logger.LogInformation("Успешный вход в аккаунт {login}", user.Value.UserName);
     
        return ApplicationExecuteLogicResult<AuthTokensPair>.Success(authPair.Value!).WithWarnings(warnings);
    }

    /// <summary> Процесс получения свежей auth пары по refresh токен </summary>
    public async Task<ApplicationExecuteLogicResult<AuthTokensPair>> RefreshAuthPairAsync(string refreshToken)
    {
        _logger.LogInformation("Попытка создать свежую пару auth токенов по refresh токен {token}", refreshToken);
        
        var warnings = new List<ApplicationError>();

        var userId = await _tokenService.GetUserIdByRefreshTokenBody(refreshToken);
        if (userId.IsSuccess is not true)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure().Merge(userId);
        
        var user = await _userService.UserByLoginOrIdAsync(userId.Value.ToString());
        if (user.IsSuccess is not true)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure().Merge(user);
        
        var roles = await _roleService.GetRolesByUser(user.Value!);
        if (user.IsSuccess is not true)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure().Merge(roles);
        
        // Создаем свежую пару auth токенов
        var pair = await _tokenService.RegenerateAuthTokensPairAsync(refreshToken, user.Value!, roles.Value!, 15);
        if (pair.IsSuccess is not true)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure().Merge(pair);
        warnings.AddRange(pair.GetWarnings);

        return ApplicationExecuteLogicResult<AuthTokensPair>.Success(pair.Value!).WithWarnings(warnings);
    }

    /// <summary> Процесс выхода пользователя из аккаунта/всех аккаунтов </summary>
    public async Task<ApplicationExecuteLogicResult<Unit>> LogoutUserAsync(ClaimsPrincipal claims, bool globally)
    {
        _logger.LogInformation("Попытка {try} выхода пользователя", globally ? "глобального" : "локального");
        
        var warnings = new List<ApplicationError>();
        
        // Для всех аккаунтов пользователя
        if (globally)
        {
            // Находим пользователя из UserId в JwtClaims 
            var rawIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
            if (rawIdClaim is null)
                return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(
                    JwtTokenErrors.UserIdNotFoundInClaims, "JwtToken не содержит UserId", 
                    "Из claims не удалось получить UserId", ErrorSeverity.Critical, HttpStatusCode.Forbidden));
            
            var userId = Guid.Parse(rawIdClaim.Value);
            
            var user = await _userService.UserByLoginOrIdAsync(userId.ToString());
            if (user.IsSuccess is not true)
                return ApplicationExecuteLogicResult<Unit>.Failure().Merge(user);
            warnings.AddRange(user.GetWarnings);
            
            // Выходим глобально
            var logout = await LogoutGlobally(user.Value!);
            if (logout.IsSuccess is not true)
                return ApplicationExecuteLogicResult<Unit>.Failure().Merge(logout);
            warnings.AddRange(logout.GetWarnings);
        }
        else // Для конкретного
        {
            // Считаем Jti и exp
            var jti = claims.FindFirstValue(JwtRegisteredClaimNames.Jti);
            if (string.IsNullOrWhiteSpace(jti))
                return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(
                    JwtTokenErrors.JtiNotFoundInClaims, "JwtToken не содержит Jti", 
                    "Из claims не удалось получить Jti", ErrorSeverity.Critical, HttpStatusCode.Forbidden));
            
            var rawExp = claims.FindFirstValue(JwtRegisteredClaimNames.Exp);
            if (string.IsNullOrWhiteSpace(rawExp))
                warnings.Add(new ApplicationError(
                    JwtTokenErrors.ExpNotFoundInClaims, "JwtToken не содержит Exp", 
                    "Из claims не удалось получить Exp", ErrorSeverity.NotImportant));
            
            var exp = Helper.StringExpirationToDateTime(rawExp!);
            if (exp.GetWarnings.Count > 0) // Если получили ошибки, значит преобразовать exp - не получилось, ставим свой
            {
                exp.Value = DateTimeOffset.FromUnixTimeSeconds(DateTime.Now.AddHours(24).Second).UtcDateTime;
                warnings.AddRange(exp.GetWarnings);
            }
            
            // Выходим локально
            var logout = await LoginConcreteAccount(jti, exp.Value);
            if (logout.IsSuccess is not true)
                return ApplicationExecuteLogicResult<Unit>.Failure().Merge(logout);
            warnings.AddRange(logout.GetWarnings);
        }
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value).WithWarnings(warnings);
    }

    #region Вспомогательные методы

    public async Task<ApplicationExecuteLogicResult<Unit>> LogoutGlobally(ApplicationUserEntity user)
    {
        var warnings = new List<ApplicationError>();
        
        // Отозвать все refresh токены
        var revokedAll = await _tokenService.RevokeAllUserRefreshTokensAsync(Guid.Parse(user.Id));
        if (revokedAll.IsSuccess is not true)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(revokedAll);
        warnings.AddRange(revokedAll.GetWarnings);
        
        // Обновить security stamp 
        var updatedStamp = await _userService.UpdateUserSecurityStampAsync(user);
        if (updatedStamp.IsSuccess is not true)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(updatedStamp);
        warnings.AddRange(updatedStamp.GetWarnings);

        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value).WithWarnings(warnings);
    }

    private async Task<ApplicationExecuteLogicResult<Unit>> LoginConcreteAccount(string jti, DateTime expiration)
    {
        var warnings = new List<ApplicationError>();
        
        // Получаем refresh токен
        var refresh = await _tokenService.GetRefreshTokenByAccessJtiAsync(jti);
        if (refresh.IsSuccess is not true)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(refresh);
        warnings.AddRange(refresh.GetWarnings);
        
        // Отозвать refresh токен связанный с access 
        var revokedRefresh = await _tokenService.RevokeConcreteUserRefreshTokenAsync(refresh.Value!);
        if (revokedRefresh.IsSuccess is not true)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(revokedRefresh);
        warnings.AddRange(revokedRefresh.GetWarnings);

        // Внести Access токен в черный список
        var bannedAccess = await _tokenService.RevokeAccessTokenByJtiAsync(jti, expiration);
        if (bannedAccess.IsSuccess is not true)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(bannedAccess);
        warnings.AddRange(bannedAccess.GetWarnings);
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value).WithWarnings(warnings);
    }

    #endregion
}