using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Private.Services.ErrorHelpers;
using Private.Services.Repositories;
using Private.ServicesInterfaces;
using Private.StorageModels;
using Public.Models.ApplicationErrors;
using Public.Models.BusinessModels.TokenModels;
using Public.Models.CommonModels;
using Public.Models.Extensions;

namespace Private.Services.TokenServices;

public class HandmadeTokenService : ITokenService
{
    private const string EmailTokenName = "EmailConfirmationToken";
    private const string RefreshTokenTokenName = "RefreshToken";
    private const string AccessTokenTokenName = "AccessToken";
    
    public const string IdentitySecurityStamp = "stp";

    private const string SigningKey = "d1e4L3zb1b9qF/gmXLdmE1op6mYImVU4VfW+HjNh3iA=";
    private const int DaysRefreshTokenLive = 7;
    
    private readonly ILogger<HandmadeTokenService> _logger;
    
    private readonly IEmailConfirmationTokenRepository _emailConfirmationTokenRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IBlackListAccessTokenRepository _blackListAccessTokenRepository;

    public HandmadeTokenService(IEmailConfirmationTokenRepository emailConfirmationTokenRepository, IRefreshTokenRepository refreshTokenRepository,
        IBlackListAccessTokenRepository blackListAccessTokenRepository, ILogger<HandmadeTokenService> logger)
    {
        _logger = logger;
        
        _emailConfirmationTokenRepository = emailConfirmationTokenRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _blackListAccessTokenRepository = blackListAccessTokenRepository;
    }
    
    public async Task<ApplicationExecuteLogicResult<string>> GenerateConfirmEmailTokenAsync(Guid userId, DateTime expiresAt)
    {
        _logger.LogInformation("Создание и сохранение токена для подтверждения почты для пользователя с id {id}", userId);

        var tokenBody = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var tokenHash = Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(tokenBody)));

        var entity = new EmailConfirmationTokenEntity
        {
            UserId = userId.ToString(),
            TokenBody = tokenHash,
            ExpiresAt = expiresAt,
        };

        _logger.LogInformation("Сохранение токена в хранилище");

        var created = await _emailConfirmationTokenRepository.SaveEmailConfirmationTokenAsync(entity);
        if (created.IsSuccess is not true)
        {
            if (created.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<EmailConfirmationTokenEntity, string>(EmailTokenErrors.TokenNotCreated, created);
        }
        
        _logger.LogInformation("Токен подтверждения почты с id {id} был сохранен", created.Value!.Id);

        return ApplicationExecuteLogicResult<string>.Success(created.Value!.TokenBody).Merge(created);
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> CheckEmailConfirmationTokenAsync(Guid userId, string token)
    {
        _logger.LogInformation("Проверка токена {token} для подтверждения почты для пользователя с id {id}", token, userId);
     
        var warnings = new List<ApplicationError>();
        
        var entity = await _emailConfirmationTokenRepository.GetEmailConfirmationTokenByBodyAsync(token);
        if (entity.IsSuccess is not true)
        {
            if (entity.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<EmailConfirmationTokenEntity, Unit>(EmailTokenErrors.TokenNotFound, entity);
            
            if (entity.ContainsError(DatabaseErrors.NotFound))
                return ErrorHelper.WrapNotFoundError<EmailConfirmationTokenEntity, Unit>(EmailTokenErrors.TokenNotFound, EmailTokenName, token, entity);
        }
        
        if (entity.Value!.Confirmed)
            warnings.Add(new ApplicationError(EmailTokenErrors.AlreadyConfirmed, "Токен уже подтвержден",
                $"По переданному токену уже было подтверждение", ErrorSeverity.NotImportant));

        if (entity.Value.UserId != userId.ToString() || entity.Value.ExpiresAt > DateTime.UtcNow)
            return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(EmailTokenErrors.IncorrectUserOrExpired, "Некорректный токен",
                $"Пользователи токена не совпадают либо токен уже истек", ErrorSeverity.Critical, HttpStatusCode.BadRequest))
                .WithWarnings(warnings);
        
        entity.Value.Confirmed = true;
        entity.Value.ConfirmedAt= DateTime.UtcNow;
        
        var updated = await _emailConfirmationTokenRepository.RewriteEmailConfirmationTokenAsync(entity.Value);
        if (updated.ContainsError(DatabaseErrors.DatabaseException))
            return ErrorHelper.WrapDbExceptionError<EmailConfirmationTokenEntity, Unit>(EmailTokenErrors.TokenNotCreated, updated).WithWarnings(warnings);

        _logger.LogInformation("Проверка токена {token} для подтверждения почты для пользователя с id {id} успешна", token, userId);
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value).WithWarnings(warnings);
    }

    public async Task<ApplicationExecuteLogicResult<AuthTokensPair>> GenerateAuthTokensPairAsync(ApplicationUserEntity user, List<string> roles, int ttlMinutes)
    {
        _logger.LogInformation("Генерация пары токенов авторизации для пользователя {login}", user.UserName);
        
        var now = DateTime.UtcNow;
        
        var jti = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName!),
            new (JwtRegisteredClaimNames.Jti, jti),
            new (IdentitySecurityStamp, user.SecurityStamp!)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey))
        {
            KeyId = "main-key"
        };
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var jwtToken = new JwtSecurityToken(
            issuer: "car-service",
            audience: "unknown",
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(ttlMinutes),
            signingCredentials: creds
        );
        
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        
        _logger.LogDebug("Созданный jwt- {jwt}, access - {access}, refresh - {refresh}", jwtToken, accessToken, refreshToken);

        var entity = new RefreshTokenEntity
        {
            Jti = jti,
            UserId = user.Id,
            RefreshTokenBody = refreshToken,
            ExpiresAtUtc = now.AddDays(DaysRefreshTokenLive),
        };

        var saved = await _refreshTokenRepository.SaveRefreshTokenAsync(entity);
        if (saved.IsSuccess is not true)
        {
            if (saved.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<RefreshTokenEntity, AuthTokensPair>(RefreshTokenErrors.TokenNotCreated, saved);
        }
        
        _logger.LogInformation("Пара токенов авторизации для пользователя {id} успешно создана", user.Id);
        
        return ApplicationExecuteLogicResult<AuthTokensPair>.Success(new AuthTokensPair
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        });
    }

    public async Task<ApplicationExecuteLogicResult<AuthTokensPair>> RegenerateAuthTokensPairAsync(string refreshToken, ApplicationUserEntity user, List<string> roles, int ttlMinutes)
    {
        _logger.LogInformation("Перегенерация пары токенов авторизации для пользователя {id}", user.Id);
        
        var warnings = new List<ApplicationError>();
        
        // Ищем запись о переданном refresh 
        var refresh = await _refreshTokenRepository.GetRefreshTokenByBodyAsync(refreshToken);
        if (refresh.IsSuccess is not true)
        {
            if (refresh.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<RefreshTokenEntity, AuthTokensPair>(RefreshTokenErrors.TokenNotFound, refresh);
            
            if (refresh.ContainsError(DatabaseErrors.NotFound))
                return ErrorHelper.WrapNotFoundError<RefreshTokenEntity, AuthTokensPair>(RefreshTokenErrors.TokenNotFound, RefreshTokenTokenName, refreshToken, refresh);
        }
        
        // Истек ли refresh токен
        if (refresh.Value!.ExpiresAtUtc < DateTime.UtcNow)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure(new ApplicationError(RefreshTokenErrors.TokenExpired, "Refresh токен истек",
                "Refresh токен истек, необходимо повторно авторизоваться", ErrorSeverity.Critical, HttpStatusCode.BadRequest)).Merge(refresh);
        
        // Удаляем старый refresh токен
        var deleted = await _refreshTokenRepository.DeleteRefreshTokenByIdAsync(refresh.Value.Id);
        if (deleted.IsSuccess is not true)
            if (refresh.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<RefreshTokenEntity, AuthTokensPair>(RefreshTokenErrors.TokenNotDeleted, refresh);
        
        // Создаем новую пару
        var pair = await GenerateAuthTokensPairAsync(user, roles, ttlMinutes);
        if (pair.IsSuccess is not true)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure().Merge(pair);
        
        return ApplicationExecuteLogicResult<AuthTokensPair>.Success(pair.Value!); 
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> RevokeAllUserRefreshTokensAsync(Guid userId)
    {
        _logger.LogInformation("Удаление всех refresh токенов авторизации для пользователя {id}", userId);
        
        var deleted = await _refreshTokenRepository.DeleteAllUserRefreshTokensAsync(userId);
        if (deleted.IsSuccess is not true)
        {
            if (deleted.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<Unit, Unit>(RefreshTokenErrors.TokenNotDeleted, deleted);
            
            if (deleted.ContainsError(DatabaseErrors.NotFound))
                return ErrorHelper.WrapNotFoundError<Unit, Unit>(RefreshTokenErrors.TokenNotDeleted, RefreshTokenTokenName, userId.ToString(), deleted);
        }
        
        _logger.LogInformation("Все refresh токены для {id} удалены", userId);
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> RevokeConcreteUserRefreshTokenAsync(string refreshToken)
    {
        _logger.LogInformation("Удаление записи о refresh токене по телу {body}", refreshToken);
        
        var deleted = await _refreshTokenRepository.DeleteRefreshTokenByBodyAsync(refreshToken);
        if (deleted.IsSuccess is not true)
        {
            if (deleted.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<Unit, Unit>(RefreshTokenErrors.TokenNotDeleted, deleted);
            
            if (deleted.ContainsError(DatabaseErrors.NotFound))
                return ErrorHelper.WrapNotFoundError<Unit, Unit>(RefreshTokenErrors.TokenNotDeleted, RefreshTokenTokenName, refreshToken, deleted);
        }
        
        _logger.LogInformation("Запись о refresh токене {body} удалена", refreshToken);
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> RevokeAccessTokenByJtiAsync(string jti, DateTime expiration)
    {
        _logger.LogInformation("Попытка забанить access токен по jti {jti}", jti);

        var entity = new BlackListTokenAccessEntity
        {
            Jti = jti,
            ExpiresAtUtc = expiration,
        };
        
        var banned = await _blackListAccessTokenRepository.SaveAccessTokenInBlackListAsync(entity);
        if (banned.IsSuccess is not true)
        {
            if (banned.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<BlackListTokenAccessEntity, Unit>(AccessTokenErrors.TokenNotBanned, banned);
            
            if (banned.ContainsError(DatabaseErrors.NotFound))
                return ErrorHelper.WrapNotFoundError<BlackListTokenAccessEntity, Unit>(AccessTokenErrors.TokenNotBanned, AccessTokenTokenName, jti, banned);
        }
        
        _logger.LogInformation("Попытка забанить access токен по jti {jti} успешна", jti);
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }

    public async Task<ApplicationExecuteLogicResult<bool>> AccessTokenIsRevokedByJtiAsync(string jti)
    {
        _logger.LogInformation("Попытка проверить access токен в черном список по jti {jti}", jti);

        var entity = await _blackListAccessTokenRepository.GetAccessTokenByJtiAsync(jti);
        if (entity.IsSuccess is not true)
        {
            if (entity.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<BlackListTokenAccessEntity, bool>(AccessTokenErrors.TokenNotFound, entity);
            if (entity.ContainsError(DatabaseErrors.NotFound))
                return ApplicationExecuteLogicResult<bool>.Success(false);
        }

        bool exist = entity.Value is not null;
        var warnExpired = entity.Value!.ExpiresAtUtc < DateTime.UtcNow
            ? new ApplicationError(AccessTokenErrors.BanExpired, "Access токен не в чс",
                "Срок заключения access токена в чс истек", ErrorSeverity.NotImportant)
            : null;
        
        return ApplicationExecuteLogicResult<bool>.Success(exist).WithPossiblyWarning(warnExpired);
    }

    public async Task<ApplicationExecuteLogicResult<string>> GetRefreshTokenByAccessJtiAsync(string jti)
    {
        _logger.LogInformation("Поиск refresh токена по связанному с ним jti access токена");
        
        var refresh = await _refreshTokenRepository.GetRefreshTokenByJtiAsync(jti);
        if (refresh.IsSuccess is not true)
        {
            if (refresh.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<RefreshTokenEntity, string>(RefreshTokenErrors.TokenNotFound, refresh);
            if (refresh.ContainsError(DatabaseErrors.NotFound))
                return ErrorHelper.WrapNotFoundError<RefreshTokenEntity, string>(RefreshTokenErrors.TokenNotFound, RefreshTokenTokenName, jti, refresh);
        }
        
        _logger.LogInformation("Получилось найти токен {token} с jti {jti}", refresh.Value, jti);
        
        return ApplicationExecuteLogicResult<string>.Success(refresh.Value!.RefreshTokenBody);
    }

    public async Task<ApplicationExecuteLogicResult<Guid>> GetUserIdByRefreshTokenBody(string refreshToken)
    {
        _logger.LogInformation("Поиск user id по refresh токену");
        
        var refresh = await _refreshTokenRepository.GetRefreshTokenByBodyAsync(refreshToken);
        if (refresh.IsSuccess is not true)
        {
            if (refresh.ContainsError(DatabaseErrors.DatabaseException))
                return ErrorHelper.WrapDbExceptionError<RefreshTokenEntity, Guid>(RefreshTokenErrors.TokenNotFound, refresh);
            if (refresh.ContainsError(DatabaseErrors.NotFound))
                return ErrorHelper.WrapNotFoundError<RefreshTokenEntity, Guid>(RefreshTokenErrors.TokenNotFound, RefreshTokenTokenName, refreshToken, refresh);
        }
        
        _logger.LogInformation("По refresh токену {token} получилось найти пользователя {userId}", refreshToken, refresh.Value!.UserId);
        
        return ApplicationExecuteLogicResult<Guid>.Success(Guid.Parse(refresh.Value!.UserId));
    }
}