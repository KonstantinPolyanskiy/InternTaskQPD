using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Public.Models.CommonModels;
using Public.Models.Extensions;
using Public.Models.ErrorEnums;
using Public.UseCase.Models;
using Public.UseCase.Services;

namespace Public.UseCase.UseCases.UserUseCases;

[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
public class UserUseCase
{
    private readonly ILogger<UserUseCase> _logger;
    
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly ITokenService _tokenService;
    private readonly IMailSenderService _mailSenderService;

    public UserUseCase(IUserService userService, IRoleService roleService, ITokenService tokenService,
        IMailSenderService mailSenderService, 
        ILogger<UserUseCase> logger)
    {
        _userService = userService;
        _roleService = roleService;
        _tokenService = tokenService;
        _mailSenderService = mailSenderService;
        
        _logger = logger;
    }
    
    public async Task<ApplicationExecuteLogicResult<UserRegistrationResponse>> RegistrationClientAsync(DataForUserRegistration data)
    {
        _logger.LogInformation("Попытка регистрации пользователя с логином {login}", data.Login);
        
        // Проверяем что запрашиваемая роль - Client
        if (data.RequestedUserRole is not ApplicationUserRole.Client)
            return ApplicationExecuteLogicResult<UserRegistrationResponse>.Failure(
                Helper.ForbiddenRoleApplicationError());
        
        // Что такая роль существует, если нет - создаем
        var existRole = await _roleService.RoleExistAsync(data.RequestedUserRole);
        if (existRole.Value is not true || existRole.ContainsError(RoleErrors.RoleNotFound))
        {
           var role = await _roleService.RoleCreateAsync(data.RequestedUserRole);
           if (role.IsSuccess is not true)
               return ApplicationExecuteLogicResult<UserRegistrationResponse>.Failure().Merge(role);
        }
        
        // Создаем аккаунт пользователя
        var user = await _userService.CreateUserAsync(new DataForCreateUser {Data = data});
        if (user.IsSuccess is not true || user.Value is null)
            return ApplicationExecuteLogicResult<UserRegistrationResponse>.Failure().Merge(user);
        
        var userId = Guid.Parse(user.Value.Id);
        
        // Создаем токен и отправляем ссылку-подтверждение
        var token = await _tokenService.GenerateConfirmEmailTokenAsync(userId, 24);
        if (token.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserRegistrationResponse>.Failure().Merge(token);
        
        var confirmEmail = await _mailSenderService.
        
        
    }

    public async Task<ApplicationExecuteLogicResult<UserEmailConfirmationResponse>> ConfirmEmailAsync(Guid userId, string confirmToken)
    {
        
    }
}