using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using Car.App.Models.UserModels;
using Car.App.Services.TokenService;
using Car.App.Services.UserService;
using CarService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Bridge.Auth;

namespace CarService.Controllers.ClientApi;

[ApiController]
[Route("api/auth")]
public class AuthController(IMapper mapper, 
    ITokenService tokenService, IUserService userService) : ControllerBase
{
    /// <summary> Регистрация пользователя с ролью Client </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
    {
        var newUser = await userService.CreateUserWithRoleAsync(request, ApplicationUserRole.Client);
        
        return Ok(new RegistrationResponse
        {
            Login = newUser.Login,
            Email = newUser.Email,
        });
    }

    /// <summary> Авторизация </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userService.FindUserByLoginAndCheckPassword(mapper.Map<UserLoginRequest>(request));
        
        if (user.User is null || !user.PasswordIsValid) 
            return Unauthorized("Invalid login or password");

        var token = await tokenService.GenerateTokensAsync(user.User, request.Password);
        
        return Ok(new
        {
            access_token = token.AccessToken,
            refresh_token = token.RefreshToken,
        });
    }
    
    /// <summary> Выход </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromQuery] bool all = false)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        bool logoutSuccess;

        if (!all)
        {
            var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
            var exp = User.FindFirstValue(JwtRegisteredClaimNames.Exp);
            
            logoutSuccess = await tokenService.LogoutCurrentAsync(userId, jti, exp);
        }
        else
            logoutSuccess = await tokenService.LogoutAllAsync(userId);
        
        return Ok(new
        {
            logout_success = logoutSuccess,
            logout_all = all
        });
    }
    
    /// <summary> Обновление токенов </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var newPair = await tokenService.RefreshTokenAsync(request.RefreshToken);
        
        return Ok(new
        {
            access_token = newPair.AccessToken,
            refresh_token = newPair.RefreshToken,
        });
    }
}