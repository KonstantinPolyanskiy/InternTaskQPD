using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Private.Services.ErrorHelpers;
using Private.Services.Repositories;
using Private.ServicesInterfaces;
using Private.ServicesInterfaces.Commands.AuthTokenServiceCommands;
using Private.StorageModels;
using Public.Models.ApplicationErrors;
using Public.Models.BusinessModels.TokenModels;
using Public.Models.CommonModels;
using Public.Models.Extensions;
using Settings.Common;

namespace Private.Services.AuthServices;

public class IdentityAuthTokenService(ILogger<IdentityAuthTokenService> logger, IOptions<JwtSettings> opts, 
    IRefreshTokenRepository refreshRepository, IBlackListAccessTokenRepository accessRepository, UserManager<ApplicationUserEntity> userManager) : IAuthTokenService
{
    public const string IdentitySecurityStampClaim = "stp";
    
    private const string RefreshObjectName = "Refresh Token";
    private const string AccessObjectName = "Access Token";
    
    /// <summary> <see cref="IAuthTokenService.GenerateAuthTokensPairAsync"/> </summary>
    public async Task<ApplicationExecuteLogicResult<AuthTokensPair>> GenerateAuthTokensPairAsync(ApplicationUserEntity user, List<ApplicationUserRole> roles)
    {
        logger.LogInformation("Создание токенов авторизации для {login}", user.UserName);
        
        var now = DateTime.UtcNow;
        var jti = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new(JwtRegisteredClaimNames.Jti, jti),
            new(IdentitySecurityStampClaim, user.SecurityStamp!)
        };
        
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.Value.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: opts.Value.Issuer,
            audience: opts.Value.Issuer,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(opts.Value.AccessTokenLifetimeMinutes),
            signingCredentials: creds);
        
        var access = new JwtSecurityTokenHandler().WriteToken(jwt);
        var refresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        var savingResult = await refreshRepository.SaveTokenAsync(new RefreshTokenEntity
        {
            RefreshBody = refresh,
            AccessJti = jti,
            ExpiresAtUtc = now.AddDays(opts.Value.AccessTokenLifetimeMinutes),
            UserId = user.Id,
        });
        if (savingResult.IsSuccess is false)
            return ErrorHelper.WrapAllDbErrors<RefreshTokenEntity, AuthTokensPair>(RefreshTokenErrors.TokenNotCreated, savingResult, RefreshObjectName);
        
        logger.LogInformation("Токены авторизации для {login} успешно созданы", user.UserName);

        return ApplicationExecuteLogicResult<AuthTokensPair>.Success(new AuthTokensPair
        {
            AccessToken = access,
            RefreshToken = refresh,
        });
    }
    
    /// <summary> <see cref="IAuthTokenService.RefreshAuthTokensPairAsync"/> </summary>
    public async Task<ApplicationExecuteLogicResult<AuthTokensPair>> RefreshAuthTokensPairAsync(string refreshToken, ApplicationUserEntity user, List<ApplicationUserRole> roles)
    {
        logger.LogInformation("Обновление пары токенов {login}", user.UserName);
        
        var refreshResult = await refreshRepository.TokenByBodyAsync(refreshToken);
        if (refreshResult.IsSuccess is false)
            return ErrorHelper.WrapAllDbErrors<RefreshTokenEntity, AuthTokensPair>(RefreshTokenErrors.TokenNotFoundOrBlocked, refreshResult, string.Join(" ", RefreshObjectName, refreshToken));
        var refreshEntity = refreshResult.Value!;

        if (refreshEntity.ExpiresAtUtc < DateTime.UtcNow)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure(new ApplicationError(
                RefreshTokenErrors.TokenExpired, "Refresh токен истек",
                $"Refresh токен {refreshToken} истек, авторизуйтесь повторно",
                ErrorSeverity.Critical, HttpStatusCode.Unauthorized));
        
        var newPairResult = await GenerateAuthTokensPairAsync(user, roles);
        if (refreshResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure().Merge(newPairResult);
        var pair = newPairResult.Value!;
        
        var deletedResult = await refreshRepository.DeleteByIdAsync(refreshEntity.Id);
        if (deletedResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Success(pair).WithWarning(new ApplicationError(
                RefreshTokenErrors.TokenNotDeleted, "Токен не удален",
                $"Новая пара токенов успешно создана, но старый refresh не удален",
                ErrorSeverity.NotImportant));
        
        return ApplicationExecuteLogicResult<AuthTokensPair>.Success(pair);
    }
   
    /// <summary> <see cref="IAuthTokenService.RevokeAccessTokenAsync"/> </summary>  
    public async Task<ApplicationExecuteLogicResult<Unit>> RevokeAccessTokenAsync(BlockAccessTokenCommand command)
    {
        if (command.LogoutAll)
        {
            if (command.User == null)
                return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(
                    AccessTokenErrors.TokensNotRevokedBecauseUserIsNull,"Access токены не отозваны",
                    "Нет пользователя для блокировки access токенов",
                    ErrorSeverity.Critical, HttpStatusCode.BadRequest));
            
            var updatingResult = await userManager.UpdateSecurityStampAsync(command.User);
            if (updatingResult.Succeeded is false)
            {
                logger.LogError(updatingResult.Errors.ToList().ToString());
                return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(
                    AccessTokenErrors.TokensNotRevokedBecauseUnknownError, "Access токены не отозваны",
                    "Возникла неизвестная ошибка при глобальной блокировке access токенов",
                    ErrorSeverity.Critical, HttpStatusCode.InternalServerError));
            }

            return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
        }
        
        if (string.IsNullOrEmpty(command.AccessJti))
            return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(
                AccessTokenErrors.TokenNotRevokedBecauseJtiIsNull, "Access токен не отозван",
                "Не передан Jti Access токена для точечного отзыва",
                ErrorSeverity.Critical, HttpStatusCode.BadRequest));

        var blockedResult = await accessRepository.SaveTokenInBlackListAsync(new BlackListTokenAccessEntity
        {
            Jti = command.AccessJti,
            Reason = command.Reason ?? "Неизвестная причина блокировки",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(opts.Value.AccessTokenLifetimeMinutes)
        });
        if (blockedResult.IsSuccess is false)
            return ErrorHelper.WrapAllDbErrors<BlackListTokenAccessEntity, Unit>(AccessTokenErrors.TokenNotRevokedUnknownError, blockedResult, string.Join(" ", AccessObjectName, command.AccessJti));
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }

    /// <summary> <see cref="IAuthTokenService.BlockRefreshTokenAsync"/> </summary>  
    public async Task<ApplicationExecuteLogicResult<Unit>> BlockRefreshTokenAsync(BlockRefreshTokenCommand command)
    {
        if (command.LogoutAll)
        {
            var deletedAllResult = await refreshRepository.DeleteAllUsersTokens(command.User.Id);
            if (deletedAllResult.IsSuccess is false)
                return ErrorHelper.WrapAllDbErrors<Unit, Unit>(RefreshTokenErrors.TokenNotDeleted, deletedAllResult, string.Join(" ", RefreshObjectName, "по user id", command.User));
            
            return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
        }
        
        var tokenResult = await refreshRepository.TokenByUserId(command.User.Id);
        if (tokenResult.IsSuccess is false)
            return ErrorHelper.WrapAllDbErrors<RefreshTokenEntity, Unit>(RefreshTokenErrors.TokenNotDeleted, tokenResult, string.Join(" ", RefreshObjectName, "по user id", command.User));
        var token = tokenResult.Value!;
        
        var deletedOnceResult = await refreshRepository.DeleteByIdAsync(token.Id);
        if (deletedOnceResult.IsSuccess is false)
            return ErrorHelper.WrapAllDbErrors<RefreshTokenEntity, Unit>(RefreshTokenErrors.TokenNotDeleted, tokenResult, string.Join(" ", RefreshObjectName, token.Id));
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }

    /// <summary> <see cref="IAuthTokenService.AccessTokenIsBlockedAsync"/> </summary>
    public async Task<ApplicationExecuteLogicResult<bool>> AccessTokenIsBlockedAsync(string jti)
    {
        var accessTokenResult = await accessRepository.TokenByJtiAsync(jti);
        if (accessTokenResult.IsSuccess is false)
        {
            if (accessTokenResult.ContainsError(DatabaseErrors.NotFound))
                return ApplicationExecuteLogicResult<bool>.Success(false);
            return ErrorHelper.WrapAllDbErrors<BlackListTokenAccessEntity, bool>(AccessTokenErrors.UnknownError, accessTokenResult, string.Join(" ", AccessObjectName, jti));
        }
        var token = accessTokenResult.Value;
        
        bool exist = token is not null;
        var warnExpired = token!.ExpiresAtUtc < DateTime.UtcNow
            ? new ApplicationError(AccessTokenErrors.BanExpired, "Access токен не в чс",
                "Срок заключения access токена в чс истек", ErrorSeverity.NotImportant)
            : null;
        
        return ApplicationExecuteLogicResult<bool>.Success(exist).WithPossiblyWarning(warnExpired);
    }
}