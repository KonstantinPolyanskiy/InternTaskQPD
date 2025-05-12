using System.Net;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Private.ServicesInterfaces;
using Public.Models.ApplicationErrors;
using Public.Models.CommonModels;
using Public.Models.DtoModels.UserDtoModels;
using Public.Models.Extensions;
using Public.UseCase.Models;
using Public.UseCase.UseCases.UserUseCases;

namespace Public.UseCase.UseCases.ConsumerUseCases;

public class ConsumerUseCases
{
    private readonly ILogger<UserUseCase> _logger;
    private readonly IMapper _mapper;
    
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly IMailSenderService _mailSenderService;
    
    
    // ReSharper disable once ConvertToPrimaryConstructor
    public ConsumerUseCases(IUserService userService, IMapper mapper, ITokenService tokenService,
        IMailSenderService mailSenderService, 
        ILogger<UserUseCase> logger)
    {
        
        _userService = userService;
        _tokenService = tokenService;
        _mailSenderService = mailSenderService;

        _mapper = mapper;
        _logger = logger;
    }
    
    /// <summary> Процесс регистрации клиента </summary>
    public async Task<ApplicationExecuteLogicResult<UserRegistrationResponse>> RegistrationClientAsync(DataForConsumerRegistration data)
    {
        _logger.LogInformation("Попытка регистрации пользователя с логином {login}", data.Login);
        
        var warnings = new List<ApplicationError>();
        
        // Проверяем что запрашиваемая роль - Client
        if (data.RequestedUserRole is not ApplicationUserRole.Client)
            return ApplicationExecuteLogicResult<UserRegistrationResponse>.Failure(
                Helper.ForbiddenRoleApplicationError());
        
        // Создаем аккаунт пользователя
        var dataForUser = _mapper.Map<DataForCreateUser>(data);
        dataForUser.InitialRoles = [ApplicationUserRole.Client];
        
        var user = await _userService.CreateUserAsync(dataForUser);
        if (user.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserRegistrationResponse>.Failure().Merge(user);
        warnings.AddRange(user.GetWarnings);

        var setRoles = await _userService.AddRolesToUser(user.Value!, [ApplicationUserRole.Client]);
        if (setRoles.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserRegistrationResponse>.Failure().Merge(setRoles);
        
        var userId = user.Value!.Id;
        
        // Создаем токен и отправляем на почту ссылку-подтверждение
        var token = await _tokenService.GenerateConfirmEmailTokenAsync(Guid.Parse(userId), DateTime.UtcNow.AddHours(24));
        if (token.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserRegistrationResponse>.Failure().Merge(token);
        warnings.AddRange(user.GetWarnings);
        
        var url = new Uri($"/confirm-email?uid={ Uri.EscapeDataString(userId) }&code={ Uri.EscapeDataString(token.Value!) }", UriKind.Relative);

        var confirmEmail = await _mailSenderService.SendConfirmationEmailAsync(user.Value.Email!, url.ToString());
        if (confirmEmail.IsSuccess is not true)
            return ApplicationExecuteLogicResult<UserRegistrationResponse>.Failure().Merge(confirmEmail);
        warnings.AddRange(confirmEmail.GetWarnings);
        
        _logger.LogInformation("Успешная регистрация пользователя с логином {login}", user.Value.UserName);

        var registrationResponse = new UserRegistrationResponse
        {
            Login = user.Value.UserName!,
            Email = user.Value.Email!,
        };

        return ApplicationExecuteLogicResult<UserRegistrationResponse>.Success(registrationResponse).WithWarnings(warnings);
    }
}