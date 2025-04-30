using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Car.App.Services.TokenService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Models.Shared.User;

namespace CarService.Events;

public class JwtLogoutEvent : JwtBearerEvents
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService                _tokenService;

    public JwtLogoutEvent(
        UserManager<ApplicationUser> userManager,
        ITokenService                tokenService)
    {
        _userManager  = userManager;
        _tokenService = tokenService;

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
        var stp    = claims.FindFirstValue(SimpleTokenService.IdentitySecurityStamp);

        var user = await _userManager.FindByIdAsync(userId!);
        if (user == null || stp == null || user.SecurityStamp != stp)
        {
            ctx.Fail("Все токены отозваны");
            return;
        }

        if (!string.IsNullOrEmpty(jti) && await _tokenService.AccessTokenInBlackList(jti))
            ctx.Fail("Переданный токен отозван");
    }
}