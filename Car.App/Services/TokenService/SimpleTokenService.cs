using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Car.App.Models.Dto;
using Car.App.Models.TokenModels;
using Car.App.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Models.Bridge.Auth;
using Models.Shared.User;

namespace Car.App.Services.TokenService;

public class SimpleTokenService(
    IOptions<JwtSettings> opts,
    UserManager<ApplicationUser> um,
    IRefreshTokenRepository refreshTokenRepository,
    IBlacklistTokenRepository blacklistTokenRepository) : ITokenService
{
    public const string IdentitySecurityStamp = "stp";
    
    public async Task<bool> LogoutAllAsync(string userId)
    {
        // User существует
        var user = await um.FindByIdAsync(userId);
        if (user is null)
            return false;

        // Отзываем все refresh
        await RevokeAllRefreshTokenAsync(user.Id);
        
        // Инвалидируем SecurityStamp 
        await um.UpdateSecurityStampAsync(user);
        
        return true;
    }

    public async Task<bool> LogoutCurrentAsync(string userId, string? jti, string? exp)
    {
        // User существует
        var user = await um.FindByIdAsync(userId);
        if (user is null)
            return false;
        
        // Отзываем refresh токен выданный с access
        if (jti is null)
            return false;
        var refreshByAccess = await GetRefreshTokenByJtiAsync(jti);
        if (refreshByAccess is null)
            return false;
        await RevokeRefreshTokenByTokenAsync(refreshByAccess.Token);
        
        // Вносим в черный список access
        if (string.IsNullOrWhiteSpace(jti) || !long.TryParse(exp, out var expSeconds))
            return false;
        
        await RevokeAccessTokenAsync(jti, DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime);
        
        return true;
    }

    public async Task<TokenPairResponse> RefreshTokenAsync(string refreshToken)
    {
        // Есть ли такой токен вообще
        var currentRefresh = await GetRefreshTokenAsync(refreshToken);
        if (currentRefresh is null) throw new SecurityTokenException("Неизвестный refresh токен");
        
        // Не истек ли access
        if (currentRefresh.ExpiresAtUtc < DateTimeOffset.UtcNow)
            throw new SecurityTokenExpiredException("Истекший refresh токен");
        
        // Удаляем старый и получаем новый
        await RevokeRefreshTokenByTokenAsync(currentRefresh.Token);
        
        var user = await um.FindByIdAsync(currentRefresh.UserId);
        if (user is null) throw new SecurityTokenException("Неизвестный пользователь");
        
        var pair = await GenerateTokensAsync(user);

        return new TokenPairResponse
        {
            AccessToken = pair.AccessToken,
            RefreshToken = pair.RefreshToken,
        };
    }
    
    public async Task<TokenPairResponse> GenerateTokensAsync(ApplicationUser user, string? password = null)
    {
        var now = DateTime.UtcNow;

        var jti = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new (JwtRegisteredClaimNames.Jti, jti),
            new (IdentitySecurityStamp, user.SecurityStamp!)
        };

        var roles = await um.GetRolesAsync(user);
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
        await refreshTokenRepository.SaveRefreshTokenAsync(user.Id, refreshToken, jti,
            now.AddDays(opts.Value.RefreshTokenLifetimeDays));

        return new TokenPairResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };
    }
    
    public async Task<bool> AccessTokenInBlackList(string jti) =>
        await blacklistTokenRepository.InBlackList(jti);


    #region Вспомогательные методы

    private async Task<RefreshTokenResponse?> GetRefreshTokenAsync(string refreshToken)
    {
        var result = await refreshTokenRepository.GetByRefreshTokenAsync(refreshToken);
        if (result is null)
            return null;
        
        return new RefreshTokenResponse
        {
            UserId = result.UserId,
            Token = result.Token,
            ExpiresAtUtc = result.ExpiresAtUtc,
        };
    }

    private async Task<RefreshTokenResponse?> GetRefreshTokenByJtiAsync(string jti)
    {
        var result = await refreshTokenRepository.GetRefreshTokenByJtiAsync(jti);
        if (result is null)
            return null;
        
        return new RefreshTokenResponse
        {
            UserId = result.UserId,
            Token = result.Token,
            ExpiresAtUtc = result.ExpiresAtUtc,
        };
    }
 
    private async Task RevokeRefreshTokenByTokenAsync(string refreshToken) =>
        await refreshTokenRepository.DeleteRefreshTokenByTokenAsync(refreshToken);

    private async Task RevokeRefreshTokenByUserIdAsync(string userId) =>
        await refreshTokenRepository.DeleteRefreshTokenByUserIdAsync(userId);
    
    private async Task RevokeAllRefreshTokenAsync(string userId) =>
        await refreshTokenRepository.DeleteAllRefreshTokensAsync(userId);
    
    private async Task RevokeAccessTokenAsync(string accessToken, DateTime expiresAt) =>
        await blacklistTokenRepository.AddBlackList(new BlacklistTokenDto {Jti = accessToken, ExpiresAt = expiresAt });
    
    #endregion
    
}