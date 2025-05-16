using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Private.Services.AuthServices;
using Private.ServicesInterfaces;
using Private.StorageModels;

namespace Public.UseCase.UseCases.UserUseCases.Events;

public class JwtLogoutEvent : JwtBearerEvents
{
    private readonly UserManager<ApplicationUserEntity> _userManager;
    private readonly IAuthTokenService                _authTokenService;

    public JwtLogoutEvent(
        UserManager<ApplicationUserEntity> userManager, IAuthTokenService authTokenService)
    {
        _userManager  = userManager;
        _authTokenService = authTokenService;

        OnTokenValidated       = ValidateAsync;
        OnAuthenticationFailed = ctx =>
        {
            ctx.HttpContext
                .RequestServices
                .GetRequiredService<ILogger<JwtLogoutEvent>>()
                .LogDebug("Провалена jwt авторизация: {Error}", ctx.Exception.Message);
            return Task.CompletedTask;
        };
    }

    private async Task ValidateAsync(TokenValidatedContext ctx)
    {
        var claims = ctx.Principal!;
        var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier);
        var jti    = claims.FindFirstValue(JwtRegisteredClaimNames.Jti);
        var stp    = claims.FindFirstValue(IdentityAuthTokenService.IdentitySecurityStampClaim);

        var user = await _userManager.FindByIdAsync(userId!);
        if (user == null || stp == null || user.SecurityStamp != stp)
        {
            ctx.Fail("Все токены отозваны");
            return;
        }
        
        if (string.IsNullOrEmpty(jti))
            return;
        
        var revoked = await _authTokenService.AccessTokenIsBlockedAsync(jti);
        
        if (revoked.Value)
            ctx.Fail("Переданный токен отозван");
    }
}