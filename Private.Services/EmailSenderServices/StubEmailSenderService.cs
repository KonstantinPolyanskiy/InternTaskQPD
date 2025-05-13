using Microsoft.Extensions.Logging;
using Private.ServicesInterfaces;
using Public.Models.CommonModels;

namespace Private.Services.EmailSenderServices;

public class StubEmailSenderService(ILogger<StubEmailSenderService> _logger) : IMailSenderService
{
    public async Task<ApplicationExecuteLogicResult<Unit>> SendConfirmationEmailAsync(string recipient, string url)
    {
        _logger.LogInformation("На почту {mail} отправлено сообщение {message}", recipient, url);
        
        return await Task.FromResult(ApplicationExecuteLogicResult<Unit>.Success(Unit.Value));
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> SendThanksForConfirmationEmailAsync(string recipient, string thanksMessage)
    {
        _logger.LogInformation("На почту {mail} отправлено сообщение {message}", recipient, thanksMessage);
        
        return await Task.FromResult(ApplicationExecuteLogicResult<Unit>.Success(Unit.Value));
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> SendAccountLoginEmailAsync(string recipient, string loginMessage)
    {
        _logger.LogInformation("На почту {mail} отправлено сообщение {message}", recipient, loginMessage);
        
        return await Task.FromResult(ApplicationExecuteLogicResult<Unit>.Success(Unit.Value));
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> SendNoPhotoNotifyEmailAsync(string recipient, string managerLogin, int carId)
    {
        _logger.LogInformation("На почту {mail} отправлено сообщение о том что менеджер {manager} добавил машину {car} без фото", recipient, managerLogin, carId);
        
        return await Task.FromResult(ApplicationExecuteLogicResult<Unit>.Success(Unit.Value));
    }
}
