using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Private.Services.Repositories;
using Private.ServicesInterfaces;
using Private.StorageModels;
using Public.Models.BusinessModels.TokenModels;
using Public.Models.CommonModels;
using Public.Models.ErrorEnums;
using Public.Models.Extensions;

namespace Private.Services.TokenServices;

public class HandmadeTokenService : ITokenService
{
    public const string IdentitySecurityStamp = "stp";

    public const string SigningKey = "sdfRT34TG34T3T34TDDFSBBBBBBBASDADFrewerwe";
    public const int DaysRefreshTokenLive = 7;
    
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

        var tokenHash = Convert.ToBase64String(SHA256.Create().ComputeHash(RandomNumberGenerator.GetBytes(32)));

        _logger.LogInformation("Сохранение токена в хранилище");

        var created = await _emailConfirmationTokenRepository.SaveEmailConfirmationTokenAsync(userId, tokenHash, expiresAt);
        if (created.IsSuccess is not true)
        {
            _logger.LogWarning("Ошибка сохранения токена подтверждения почты в хранилище");
            return ApplicationExecuteLogicResult<string>.Failure().Merge(created);
        }
        
        _logger.LogInformation("Токен подтверждения почты с id {id} был сохранен", created.Value!.Id);

        return ApplicationExecuteLogicResult<string>.Success(created.Value!.TokenBody);
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> CheckEmailConfirmationTokenAsync(Guid userId, string token)
    {
        _logger.LogInformation("Проверка токена {token} для подтверждения почты для пользователя с id {id}", token, userId);
        
        var entity = await _emailConfirmationTokenRepository.GetEmailConfirmationTokenByBodyAsync(token);
        if (entity.IsSuccess is not true || entity.Value is null)
            return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(EmailTokenErrors.TokenConfirmationNotFound, "Токен не найден",
                $"Не получилось найти запись по телу токена {token}", ErrorSeverity.Critical, HttpStatusCode.NotFound));
        
        if (entity.Value.Confirmed)
            return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(EmailTokenErrors.AlreadyConfirmed, "Токен уже подтвержден",
                $"По переданному токену уже было подтверждение", ErrorSeverity.NotImportant));

        if (entity.Value.UserId != userId || entity.Value.ExpiresAt > DateTime.UtcNow)
            return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(EmailTokenErrors.IncorrectUserOrExpired, "Некорректный токен",
                $"Пользователи токена не совпадают либо токен уже истек", ErrorSeverity.Critical, HttpStatusCode.BadRequest));
        
        entity.Value.Confirmed = true;
        entity.Value.ConfirmedAt= DateTime.UtcNow;
        
        var updated = await _emailConfirmationTokenRepository.RewriteEmailConfirmationTokenAsync(entity.Value);
        if (updated.IsSuccess is not true)
            return ApplicationExecuteLogicResult<Unit>.Failure().Merge(updated);

        _logger.LogInformation("Проверка токена {token} для подтверждения почты для пользователя с id {id} успешна", token, userId);
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }

    public async Task<ApplicationExecuteLogicResult<AuthTokensPair>> GenerateAuthTokensPairAsync(ApplicationUserEntity user, List<string> roles, int ttlMinutes)
    {
        _logger.LogInformation("Генерация пары токенов авторизации для пользователя {login}", user.UserName);
        
        var now = DateTime.UtcNow;
        
        var jti = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new (JwtRegisteredClaimNames.Jti, jti),
            new (IdentitySecurityStamp, user.SecurityStamp!)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var jwtToken = new JwtSecurityToken(
            issuer: "InternTaskQPD.CarApplication",
            audience: "UnknownAudience",
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(ttlMinutes),
            signingCredentials: creds
        );
        
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        
        _logger.LogDebug("Созданный jwt- {jwt}, access - {access}, refresh - {refresh}", jwtToken, accessToken, refreshToken);

        var saved  = await _refreshTokenRepository.SaveRefreshTokenAsync(new RefreshTokenEntity
            {
                Jti = jti,
                UserId = Guid.Parse(user.Id),
                ExpiresAtUtc = now.AddDays(DaysRefreshTokenLive),
            });
        if (saved.IsSuccess is not true)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure().Merge(saved);
        
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
        
        // Ищем запись о переданном refresh 
        var refresh = await _refreshTokenRepository.GetRefreshTokenByBodyAsync(refreshToken);
        if (refresh.IsSuccess is not true || refresh.Value is null)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure(new ApplicationError(RefreshTokenErrors.TokenNotFound, "Refresh токен не найден",
                "Не получилось найти запись о refresh токене", ErrorSeverity.Critical, HttpStatusCode.NotFound));
        
        // Истек ли refresh токен
        if (refresh.Value.ExpiresAtUtc < DateTime.UtcNow)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure(new ApplicationError(RefreshTokenErrors.TokenExpired, "Refresh токен истек",
                "Refresh токен истек, необходимо повторно авторизоваться", ErrorSeverity.Critical, HttpStatusCode.BadRequest)).Merge(refresh);
        
        // Удаляем старый refresh токен
        var deleted = await _refreshTokenRepository.DeleteRefreshTokenByIdAsync(refresh.Value.Id);
        if (deleted.IsSuccess is not true)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure(new ApplicationError(RefreshTokenErrors.TokenNotDeleted, "Refresh токен не удален",
                "По неизвестной причине refresh токен удалить не получилось", ErrorSeverity.Critical, HttpStatusCode.InternalServerError));
        
        // Создаем новую пару
        var pair = await GenerateAuthTokensPairAsync(user, roles, ttlMinutes);
        if (pair.IsSuccess is not true)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure().Merge(deleted);
        
        return ApplicationExecuteLogicResult<AuthTokensPair>.Success(pair.Value!); 
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> RevokeAllUserRefreshTokensAsync(Guid userId)
    {
        _logger.LogInformation("Удаление всех refresh токенов авторизации для пользователя {id}", userId);
        
        var deleted = await _refreshTokenRepository.DeleteAllUserRefreshTokensAsync(userId);
        if (deleted.IsSuccess is not true)
            return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(RefreshTokenErrors.TokenNotDeleted, "Refresh токены не удалены",
                "По неизвестной причине refresh токены удалить не получилось", ErrorSeverity.Critical, HttpStatusCode.InternalServerError));
        
        _logger.LogInformation("Все refresh токены для {id} удалены", userId);
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> RevokeConcreteUserRefreshTokenAsync(string refreshToken)
    {
        _logger.LogInformation("Удаление записи о refresh токене по телу {body}", refreshToken);
        
        var deleted = await _refreshTokenRepository.DeleteRefreshTokenByBodyAsync(refreshToken);
        if (deleted.IsSuccess is not true)
            return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(RefreshTokenErrors.TokenNotDeleted, "Refresh токен не удален",
                "По неизвестной причине refresh токен удалить не получилось", ErrorSeverity.Critical, HttpStatusCode.InternalServerError));
        
        _logger.LogInformation("Запись о refresh токене {body} удалена", refreshToken);
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> RevokeAccessTokenByJtiAsync(string jti, DateTime expiration)
    {
        _logger.LogInformation("Попытка забанить access токен по jti {jti}", jti);
        
        var banned = await _blackListAccessTokenRepository.AddAccessTokenToBlackListAsync(new BlackListTokenAccessEntity
        {
            Jti = jti,
            ExpiresAtUtc = expiration,
        });
        if (banned.IsSuccess is not true) 
            return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(AccessTokenErrors.TokenNotSaved, "Access токен не сохранен в ЧС",
                "По неизвестной причине access токен не получилось внести в черный список", ErrorSeverity.Critical, HttpStatusCode.InternalServerError));
        
        _logger.LogInformation("Попытка забанить access токен по jti {jti} успешна", jti);
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }

    public async Task<ApplicationExecuteLogicResult<bool>> AccessTokenIsRevokedByJtiAsync(string jti)
    {
        _logger.LogInformation("Попытка проверить access токен в черном список по jti {jti}", jti);

        var entity = await _blackListAccessTokenRepository.GetAccessTokenByJtiAsync(jti);
        if (entity.IsSuccess is not true)
            return ApplicationExecuteLogicResult<bool>.Failure(new ApplicationError(AccessTokenErrors.UnknownError, "Access токен не найден",
                "Неизвестная ошибка получения access токена", ErrorSeverity.Critical, HttpStatusCode.InternalServerError));

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
        if (refresh.IsSuccess is not true || refresh.Value is null)
            return ApplicationExecuteLogicResult<string>.Failure(new ApplicationError(RefreshTokenErrors.TokenNotFound, "Refresh токен не найден",
                "Не получилось найти запись о refresh токене", ErrorSeverity.Critical, HttpStatusCode.NotFound));
        
        _logger.LogInformation("Получилось найти токен {token} с jti {jti}", refresh.Value, jti);
        
        return ApplicationExecuteLogicResult<string>.Success(refresh.Value.RefreshTokenBody);
    }
}