using Private.StorageModels;
using Public.Models.BusinessModels.TokenModels;
using Public.Models.CommonModels;

namespace Private.ServicesInterfaces;

/// <summary> Сервис для работы с различными токенами </summary>
public interface ITokenService
{
    /// <summary> Создать токен для подтверждения Email </summary>
    public Task<ApplicationExecuteLogicResult<string>> GenerateConfirmEmailTokenAsync(Guid userId, DateTime expiresAt);
    
    /// <summary> Проверить токен для подтверждения Email </summary>
    public Task<ApplicationExecuteLogicResult<Unit>> CheckEmailConfirmationTokenAsync(Guid userId, string token);
    
    /// <summary> Создать пару токенов для авторизации </summary>
    public Task<ApplicationExecuteLogicResult<AuthTokensPair>> GenerateAuthTokensPairAsync(ApplicationUserEntity user, List<string> roles, int ttlMinutes);
    
    /// <summary> Выдать свежую пару токенов для авторизации </summary>
    public Task<ApplicationExecuteLogicResult<AuthTokensPair>> RegenerateAuthTokensPairAsync(string refreshToken, ApplicationUserEntity user, List<string> roles, int ttlMinutes);
    
    /// <summary> Отозвать все refresh токены выданные пользователю </summary>
    public Task<ApplicationExecuteLogicResult<Unit>> RevokeAllUserRefreshTokensAsync(Guid userId);
    
    /// <summary> Отозвать конкретный refresh токен пользователя </summary>
    public Task<ApplicationExecuteLogicResult<Unit>> RevokeConcreteUserRefreshTokenAsync(string refreshToken);
    
    /// <summary> Отозвать конкретный access токен пользователя </summary>
    public Task<ApplicationExecuteLogicResult<Unit>> RevokeAccessTokenByJtiAsync(string jti, DateTime expiration);
    
    /// <summary> Отозван ли access токен пользователя </summary>
    public Task<ApplicationExecuteLogicResult<bool>> AccessTokenIsRevokedByJtiAsync(string jti);
    
    /// <summary> Получить refresh токен по связанному с ним jti access токена </summary>
    public Task<ApplicationExecuteLogicResult<string>> GetRefreshTokenByAccessJtiAsync(string jti);
    
    /// <summary> Получить Id пользователя с переданным refresh токеном </summary>
    public Task<ApplicationExecuteLogicResult<Guid>> GetUserIdByRefreshTokenBody(string refreshToken);
}