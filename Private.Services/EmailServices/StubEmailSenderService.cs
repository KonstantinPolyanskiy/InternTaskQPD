using Microsoft.Extensions.Logging;
using Private.ServicesInterfaces;
using Public.Models.CommonModels;

namespace Private.Services.EmailServices;

public class StubEmailSenderService(ILogger<StubEmailSenderService> _logger) : IMailSenderService
{
    public async Task<ApplicationExecuteLogicResult<Unit>> SendAsync(string to, string subject, string body)
    {
        _logger.LogInformation("На почту {to} с темой {subject} отправлено сообщение {body}", to, subject, body);
        
        return await Task.FromResult(ApplicationExecuteLogicResult<Unit>.Success(Unit.Value));
    }
}
