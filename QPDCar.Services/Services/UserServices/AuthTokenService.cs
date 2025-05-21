using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ApplicationResult.Extensions;
using QPDCar.Models.ApplicationModels.AuthModels;
using QPDCar.Models.ApplicationModels.ErrorTypes;
using QPDCar.Models.ApplicationModels.Settings;
using QPDCar.Models.BusinessModels.EmployerModels;
using QPDCar.Models.StorageModels;
using QPDCar.Models.StorageModels.ErrorTypes;
using QPDCar.ServiceInterfaces.UserServices;
using QPDCar.Services.ErrorHelpers;
using QPDCar.Services.Repositories;

namespace QPDCar.Services.Services.UserServices;

public class AuthTokenService(IRefreshRepository refreshRepository, IBlackListAccessRepository accessRepository, 
    UserManager<ApplicationUserEntity> userManager, IOptions<JwtAuthSettings> opts, ILogger<AuthTokenService> logger) : IAuthTokenService
{
    public const string IdentitySecurityStampClaim = "stp";
    
    private const string RefreshObjectName = "Refresh Token";
    private const string AccessObjectName = "Access Token";
    public async Task<ApplicationExecuteResult<AuthTokensPair>> GenerateAuthTokensPairAsync(ApplicationUserEntity user, List<ApplicationRoles> roles)
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
            audience: opts.Value.Audience,
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
            return DbErrorHelper.WrapAllDbErrors<RefreshTokenEntity, AuthTokensPair>(RefreshTokenErrors.TokenNotCreated, savingResult, RefreshObjectName);
        
        logger.LogInformation("Токены авторизации для {login} успешно созданы", user.UserName);

        return ApplicationExecuteResult<AuthTokensPair>.Success(new AuthTokensPair
        {
            AccessToken = access,
            RefreshToken = refresh,
        });
    }

    public async Task<ApplicationExecuteResult<AuthTokensPair>> RefreshAuthTokensPairAsync(string refreshToken, ApplicationUserEntity user, List<ApplicationRoles> roles)
    {
        logger.LogInformation("Обновление пары токенов {login}", user.UserName);
        
        var refreshResult = await refreshRepository.TokenByBodyAsync(refreshToken);
        if (refreshResult.IsSuccess is false)
            return DbErrorHelper.WrapAllDbErrors<RefreshTokenEntity, AuthTokensPair>(RefreshTokenErrors.TokenNotFoundOrBlocked, refreshResult, string.Join(" ", RefreshObjectName, refreshToken));
        var refreshEntity = refreshResult.Value!;

        if (refreshEntity.ExpiresAtUtc < DateTime.UtcNow)
            return ApplicationExecuteResult<AuthTokensPair>
                .Failure(TokenErrorHelper
                    .ErrorRefreshTokenExpiredWarning(refreshToken)
                    .ToCritical(HttpStatusCode.Unauthorized));
        
        var newPairResult = await GenerateAuthTokensPairAsync(user, roles);
        if (refreshResult.IsSuccess is false)
            return ApplicationExecuteResult<AuthTokensPair>.Failure().Merge(newPairResult);
        var pair = newPairResult.Value!;
        
        var deletedResult = await refreshRepository.DeleteByIdAsync(refreshEntity.Id);
        if (deletedResult.IsSuccess is false)
            return ApplicationExecuteResult<AuthTokensPair>
                .Success(pair)
                .WithWarning(TokenErrorHelper.ErrorRefreshTokenNotDeletedWarning(refreshToken));
        
        return ApplicationExecuteResult<AuthTokensPair>.Success(pair);
    }

    public async Task<ApplicationExecuteResult<Unit>> RevokeAccessTokenAsync(ApplicationUserEntity? user, string jti, string reason, bool global)
    {
        if (global)
        {
            if (user is null)
                return ApplicationExecuteResult<Unit>.Failure(new ApplicationError(
                    UserErrors.UserNotFound,"Access токены не отозваны",
                    "Нет пользователя для блокировки access токенов",
                    ErrorSeverity.Critical, HttpStatusCode.BadRequest));
            
            var updatingResult = await userManager.UpdateSecurityStampAsync(user);
            if (updatingResult.Succeeded is false)
            {
                logger.LogError(updatingResult.Errors.ToList().ToString());
                return ApplicationExecuteResult<Unit>.Failure(new ApplicationError(
                    AccessTokenErrors.UnknownError, "Access токены не отозваны",
                    "Возникла неизвестная ошибка при глобальной блокировке access токенов",
                    ErrorSeverity.Critical, HttpStatusCode.InternalServerError));
            }
            
            return ApplicationExecuteResult<Unit>.Success(Unit.Value);
        }
        
        if (string.IsNullOrEmpty(jti))
            return ApplicationExecuteResult<Unit>.Failure(new ApplicationError(
                AccessTokenErrors.UnknownError, "Access токен не отозван",
                "Не передан Jti Access токена для точечного отзыва",
                ErrorSeverity.Critical, HttpStatusCode.BadRequest));
        
        var blockedResult = await accessRepository.SaveTokenInBlackListAsync(new BlackListAccessTokenEntity
        {
            Jti = jti,
            Reason = reason,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(opts.Value.AccessTokenLifetimeMinutes)
        });
        if (blockedResult.IsSuccess is false)
            return DbErrorHelper.WrapAllDbErrors<BlackListAccessTokenEntity, Unit>(AccessTokenErrors.UnknownError, blockedResult, string.Join(" ", AccessObjectName, jti));
        
        return ApplicationExecuteResult<Unit>.Success(Unit.Value);
    }

    public async Task<ApplicationExecuteResult<Unit>> BlockRefreshTokenAsync(ApplicationUserEntity user, string reason, bool global)
    {
        if (global)
        {
            var deletedAllResult = await refreshRepository.DeleteAllUsersTokens(user.Id);
            if (deletedAllResult.IsSuccess is false)
                return DbErrorHelper.WrapAllDbErrors<Unit, Unit>(RefreshTokenErrors.TokenNotDeleted, deletedAllResult, string.Join(" ", RefreshObjectName, "по user id", user.Id));
            
            return ApplicationExecuteResult<Unit>.Success(Unit.Value);
        }
        
        var tokenResult = await refreshRepository.TokenByUserId(user.Id);
        if (tokenResult.IsSuccess is false)
            return DbErrorHelper.WrapAllDbErrors<RefreshTokenEntity, Unit>(RefreshTokenErrors.TokenNotDeleted, tokenResult, string.Join(" ", RefreshObjectName, "по user id", user.Id));
        var token = tokenResult.Value!;
        
        var deletedOnceResult = await refreshRepository.DeleteByIdAsync(token.Id);
        if (deletedOnceResult.IsSuccess is false)
            return DbErrorHelper.WrapAllDbErrors<RefreshTokenEntity, Unit>(RefreshTokenErrors.TokenNotDeleted, tokenResult, string.Join(" ", RefreshObjectName, token.Id));
        
        return ApplicationExecuteResult<Unit>.Success(Unit.Value);
    }

    public async Task<ApplicationExecuteResult<bool>> AccessTokenIsBlockedAsync(string jti)
    {
        var accessTokenResult = await accessRepository.TokenByJtiAsync(jti);
        if (accessTokenResult.IsSuccess is false)
        {
            if (accessTokenResult.ContainsError(DatabaseErrors.EntityByIdNotFound))
                return ApplicationExecuteResult<bool>.Success(false);
            return DbErrorHelper.WrapAllDbErrors<BlackListAccessTokenEntity, bool>(AccessTokenErrors.UnknownError, accessTokenResult, string.Join(" ", AccessObjectName, jti));
        }
        var token = accessTokenResult.Value;
        
        bool exist = token is not null;
        var warnExpired = token!.ExpiresAtUtc < DateTime.UtcNow
            ? new ApplicationError(AccessTokenErrors.BanExpired, "Access токен не в чс",
                "Срок заключения access токена в чс истек", ErrorSeverity.NotImportant)
            : null;
        
        return ApplicationExecuteResult<bool>.Success(exist).WithPossiblyWarning(warnExpired);
    }
}