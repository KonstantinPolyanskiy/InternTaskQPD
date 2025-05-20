using System.Net;
using Microsoft.AspNetCore.Identity;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ErrorTypes;
using QPDCar.Models.StorageModels;
using QPDCar.ServiceInterfaces.MailServices;

namespace QPDCar.Services.Services.UserServices;

public class MailConfirmationService(UserManager<ApplicationUserEntity> userManager) : IMailConfirmationService 
{
    public async Task<ApplicationExecuteResult<string>> CreateConfirmationTokenAsync(ApplicationUserEntity user)
    {
        string token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        
        return ApplicationExecuteResult<string>.Success(token);
    }

    public async Task<ApplicationExecuteResult<Unit>> ConfirmEmailAsync(ApplicationUserEntity user, string token)
    {
        var result = await userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded is false)
            return ApplicationExecuteResult<Unit>.Failure(new ApplicationError(
                EmailTokenErrors.IncorrectUserOrExpired, "Неверный или просроченный токен",
                $"Не удалось подтвердить почту {user.UserName} по токену {token}",
                ErrorSeverity.Critical, HttpStatusCode.BadRequest));
        
        return ApplicationExecuteResult<Unit>.Success(Unit.Value);
    }
}