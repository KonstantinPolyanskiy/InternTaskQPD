using AutoMapper;
using Car.App.Models.UserModels;
using Car.App.Services.TokenService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Bridge.Auth;

namespace CarService.Controllers.ClientApi;

[ApiController]
[Route("/api/auth")]
public class AuthController(ITokenService tokenService, IMapper mapper, ) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
    {
        
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login()
    {
        
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        
    }
    
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        
    }
}