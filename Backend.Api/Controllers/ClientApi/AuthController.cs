using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using Backend.Api.Models.Requests;
using Backend.Api.Models.Responses;
using Backend.App.Models.Commands;
using Backend.App.Services.TokenService;
using Backend.App.Services.UserService;
using Enum.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.ClientApi;

[ApiController]
[Route("api/auth")]
public class AuthController(IMapper mapper, 
    ITokenService tokenService, IUserService userService) : ControllerBase
{
    /// <summary> Регистрация пользователя с ролью Client </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
    {
        var cmd = mapper.Map<CreateUserCommand>(request);
        cmd.Role = ApplicationUserRole.Client;
        
        var newUser = await userService.CreateUserWithRoleAsync(cmd);
        
        var response = mapper.Map<CreateUserResponse>(newUser);
        response.Role = ApplicationUserRole.Client.ToString();
        
        return Ok(response);
    }

    /// <summary> Авторизация </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var cmd = mapper.Map<LoginUserCommand>(request);
        
        var user = await userService.FindUserByLoginAsync(cmd);
        if (user is null)
            return Unauthorized("invalid login");

        var passwordIsValid = await userService.CheckUserPasswordAsync(cmd);
        
        if (passwordIsValid is false)
            return Unauthorized("invalid password");

        var pairCmd = mapper.Map<GenerateTokenPairCommand>(user);

        var token = await tokenService.GenerateTokensAsync(pairCmd);
        
        return Ok(mapper.Map<TokenPairResponse>(token));
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

        var cmd = new LogoutCommand { UserId = userId };

        if (!all)
        {
            cmd.LogoutAll = false;
            cmd.Jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
            cmd.RawExpiration = User.FindFirstValue(JwtRegisteredClaimNames.Exp);
            
            logoutSuccess = await tokenService.LogoutAsync(cmd);
        }
        else
        {
            cmd.LogoutAll = true;
            logoutSuccess = await tokenService.LogoutAsync(cmd);
        }
        
        return Ok(new LogoutResponse
        {
            Success = logoutSuccess,
            LogoutAll = all,
        });
    }
    
    /// <summary> Обновление токенов </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenPairRequest request)
    {
        var newPair = await tokenService.RefreshTokenAsync(mapper.Map<RefreshTokenPairCommand>(request));
        
        return Ok(mapper.Map<TokenPairResponse>(newPair));
    }
}