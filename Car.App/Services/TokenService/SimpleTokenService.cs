using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Car.App.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Models.Bridge.Auth;

namespace Car.App.Services.TokenService;

public class SimpleTokenService(JwtSettings jwtSettings, UserManager<IdentityUser> um,
    IRefreshTokenRepository refreshTokenRepository) : ITokenService
{
    public async Task<TokenResponse> GenerateTokensAsync(IdentityUser user, string? password = null)
    {
        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name,           user.UserName!)
        };
        
        var roles = await um.GetRolesAsync(user);
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var jwtToken = new JwtSecurityToken(
            issuer:         jwtSettings.Issuer,
            audience:       jwtSettings.Audience,
            claims:         claims,
            notBefore:      now,
            expires:        now.AddMinutes(jwtSettings.AccessTokenLifetimeMinutes),
            signingCredentials: creds
        );
        
        
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        await refreshTokenRepository.SaveRefreshTokenAsync(user.Id, refreshToken,
            now.AddDays(jwtSettings.RefreshTokenLifetimeDays));

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };
    }

    public async Task<TokenResponse> RefreshAsync(string refreshToken)
    {
        var tokenResult = await refreshTokenRepository.GetByRefreshTokenAsync(refreshToken);
        if (tokenResult is null || tokenResult.ExpiresAtUtc < DateTime.UtcNow)
            throw new SecurityTokenException("Invalid or expired refresh token");

        var user = await um.FindByIdAsync(tokenResult.UserId);
        if (user is null) throw new SecurityTokenException("Unknown user");

        await refreshTokenRepository.DeleteRefreshTokenAsync(refreshToken);
        return await GenerateTokensAsync(user);
    }

    public Task RevokeRefreshTokenAsync(string refreshToken) => refreshTokenRepository.DeleteRefreshTokenAsync(refreshToken);
}