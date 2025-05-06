using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Backend.App.Models.Business;
using Backend.App.Models.Commands;
using Backend.App.Models.Dto;
using Backend.App.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Settings.Common;
using static System.Int64;

namespace Backend.App.Services.TokenService;

public class SimpleTokenService(
    IOptions<JwtSettings> opts, ILogger<SimpleTokenService> log,
    UserManager<ApplicationUser> um,
    IRefreshTokenRepository refreshTokenRepository, IBlacklistTokenRepository blacklistTokenRepository) : ITokenService
{
    public const string IdentitySecurityStamp = "stp";

    public async Task<bool> LogoutAsync(LogoutCommand cmd)
    {
        log.LogDebug("Данные для выхода пользователя - {data}", cmd);

        var user = await um.FindByIdAsync(cmd.UserId);
        if (user is null) return false;

        if (cmd.LogoutAll)
        {
            log.LogInformation("Попытка глобального выхода пользователя - {login}", user.UserName);
            
            // Отзываем все refresh
            await RevokeAllRefreshTokenAsync(user.Id);
            log.LogDebug("Для пользователя {login} отозваны все refresh токены", user.UserName);
        
            // Инвалидируем SecurityStamp 
            await um.UpdateSecurityStampAsync(user);
            log.LogDebug("Для пользователя {login} инвалидированы все access токены", user.UserName);
        
            log.LogInformation("Успешный глобальный выход пользователя {login}", user.UserName);
            return true;
        }
        
        log.LogInformation("Попытка локального выхода пользователя - {login}", user.UserName);
        
        if (string.IsNullOrWhiteSpace(cmd.RawExpiration) || string.IsNullOrEmpty(cmd.Jti)) return false;
        
        var refreshByAccess = await GetRefreshTokenByJtiAsync(cmd.Jti);
        if (refreshByAccess?.RefreshToken is null) return false;
        
        await RevokeRefreshTokenByTokenAsync(refreshByAccess.RefreshToken);
        log.LogDebug("Для пользователя {login} отозван refresh токен {token}", user.UserName, refreshByAccess.RefreshToken);
        
        // Вносим в черный список access
        TryParse(cmd.RawExpiration, out var expSeconds);
        await RevokeAccessTokenAsync(cmd.Jti, DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime);
        log.LogDebug("Для пользователя {login} внесен в ЧС access токен с jti {jti}", user.UserName, cmd.Jti);
        
        log.LogInformation("Успешный локальный выход пользователя {login}", user.UserName);
        return true;
    }

    public async Task<TokenPair> RefreshTokenAsync(RefreshTokenPairCommand cmd)
    {
        log.LogInformation("Попытка выдача обновленной пары токенов");
        log.LogDebug("Данные для обновления токена - {data}", cmd);
        
        // Есть ли такой токен вообще
        var currentRefresh = await GetRefreshTokenAsync(cmd.RefreshToken);
        if (currentRefresh is null) throw new SecurityTokenException("Неизвестный refresh токен");
        
        // Не истек ли access
        if (currentRefresh.ExpiresUtc < DateTimeOffset.UtcNow) throw new SecurityTokenExpiredException("Истекший refresh токен");
        
        // Удаляем старый и получаем новый
        await RevokeRefreshTokenByTokenAsync(currentRefresh.RefreshToken!);
        
        var user = await um.FindByIdAsync(currentRefresh.UserId!);
        if (user is null) throw new SecurityTokenException("Неизвестный пользователь");
        
        var pair = await GenerateTokensAsync(new GenerateTokenPairCommand {User = user});
        
        log.LogInformation("Новая пара токенов для пользователя {login} успешно выдана", user.UserName);
        log.LogDebug("Выдаваемая пара токенов - {pair}", pair);
        
        return new TokenPair
        {
            AccessToken = pair.AccessToken,
            RefreshToken = pair.RefreshToken,
        };
    }
    
    public async Task<TokenPair> GenerateTokensAsync(GenerateTokenPairCommand cmd)
    {
        log.LogInformation("Попытка сгенерировать пару");
        log.LogDebug("Данные для генерации токена - {data}", cmd);
        
        var now = DateTime.UtcNow;

        var jti = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, cmd.User.Id),
            new(ClaimTypes.Name, cmd.User.UserName!),
            new (JwtRegisteredClaimNames.Jti, jti),
            new (IdentitySecurityStamp, cmd.User.SecurityStamp!)
        };

        var roles = await um.GetRolesAsync(cmd.User);
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.Value.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwtToken = new JwtSecurityToken(
            issuer: opts.Value.Issuer,
            audience: opts.Value.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(opts.Value.AccessTokenLifetimeMinutes),
            signingCredentials: creds
        );
        
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        
        log.LogDebug("Созданный jwt- {jwt}, access - {access}, refresh - {refresh}", jwtToken, accessToken, refreshToken);
        
        var refreshDto = new RefreshTokenDto
        {
            UserId = cmd.User.Id,
            RefreshToken = refreshToken,
            Jti = jti,
            ExpiresUtc = now.AddDays(opts.Value.RefreshTokenLifetimeDays)
        };
        await refreshTokenRepository.SaveRefreshTokenAsync(refreshDto);
        
        log.LogInformation("Пара токенов для пользователя {login} сгенерирована", cmd.User.UserName);

        return new TokenPair
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };
    }
    
    public async Task<bool> AccessTokenInBlackList(string jti) =>
        await blacklistTokenRepository.InBlackList(jti);   


    #region Вспомогательные методы

    private async Task<RefreshTokenDto?> GetRefreshTokenAsync(string refreshToken)
    {
        var result = await refreshTokenRepository.GetByRefreshTokenAsync(refreshToken);
        return result;
    }

    private async Task<RefreshTokenDto?> GetRefreshTokenByJtiAsync(string jti)
    {
        var result = await refreshTokenRepository.GetRefreshTokenByJtiAsync(jti);
        return result;
    }
 
    private async Task RevokeRefreshTokenByTokenAsync(string refreshToken) =>
        await refreshTokenRepository.DeleteRefreshTokenByTokenAsync(refreshToken);
    
    private async Task RevokeAllRefreshTokenAsync(string userId) =>
        await refreshTokenRepository.DeleteAllRefreshTokensAsync(userId);
    
    private async Task RevokeAccessTokenAsync(string accessToken, DateTime expiresAt) =>
        await blacklistTokenRepository.AddBlackList(new BlacklistTokenDto {Jti = accessToken, ExpiresAt = expiresAt });
    
    #endregion
}