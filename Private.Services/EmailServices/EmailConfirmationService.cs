using System.Net;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Private.ServicesInterfaces;
using Private.StorageModels;
using Public.Models.ApplicationErrors;
using Public.Models.CommonModels;
using Public.Models.Extensions;

namespace Private.Services.EmailServices;

public class EmailConfirmationService(UserManager<ApplicationUserEntity> userManager) : IEmailConfirmationService
{
    public async Task<ApplicationExecuteLogicResult<string>> CreateConfirmationTokenAsync(ApplicationUserEntity user)
    {
        string token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        
        return ApplicationExecuteLogicResult<string>.Success(token);
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> ConfirmEmailAsync(ApplicationUserEntity user, string token)
    {
        var result = await userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded is false)
            return ApplicationExecuteLogicResult<Unit>.Failure(new ApplicationError(
                EmailTokenErrors.IncorrectUserOrExpired, "Неверный или просроченный токен",
                $"Не удалось подтвердить почту {user.UserName} по токену {token}",
                ErrorSeverity.Critical, HttpStatusCode.BadRequest));
        
        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value);
    }
}