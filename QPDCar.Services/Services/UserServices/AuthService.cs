using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ApplicationResult.Extensions;
using QPDCar.Models.ApplicationModels.AuthModels;
using QPDCar.Models.ApplicationModels.ErrorTypes;
using QPDCar.Models.BusinessModels.EmployerModels;
using QPDCar.Models.StorageModels;
using QPDCar.ServiceInterfaces.UserServices;
using QPDCar.Services.ErrorHelpers;

namespace QPDCar.Services.Services.UserServices;

public class AuthService(SignInManager<ApplicationUserEntity> signInManager, IRoleService roleService,
    IAuthTokenService authTokenService, IHttpContextAccessor contextAccessor) : IAuthService
{
    public async Task<ApplicationExecuteResult<AuthTokensPair>> SignInAndGetAuthTokensAsync(string login, string password)
    {
        var signInResult = await signInManager.PasswordSignInAsync(login, password, false, false);
        if (signInResult.Succeeded is false)
            return ApplicationExecuteResult<AuthTokensPair>.Failure(UserErrorHelper.ErrorIncorrectLoginOrPasswordWarning());
        
        var user = await signInManager.UserManager.FindByNameAsync(login);
        if (user is null)
            return ApplicationExecuteResult<AuthTokensPair>
                .Failure(UserErrorHelper
                    .ErrorUserNotFoundWarning(login)
                    .ToCritical(HttpStatusCode.NotFound));

        var rolesResult = await roleService.GetRolesByUser(user);
        if (rolesResult.IsSuccess is false)
            return ApplicationExecuteResult<AuthTokensPair>.Failure().Merge(rolesResult);
        var roles = rolesResult.Value!;
        
        var pairResult = await authTokenService.GenerateAuthTokensPairAsync(user, roles);
        if (pairResult.IsSuccess is false)
            return ApplicationExecuteResult<AuthTokensPair>.Failure().Merge(pairResult);
        var pair = pairResult.Value!;
        
        return ApplicationExecuteResult<AuthTokensPair>.Success(pair);
    }

    public async Task<ApplicationExecuteResult<AuthTokensPair>> GetFreshTokensAsync(string refreshToken)
    { 
        var user = await signInManager.UserManager.Users
            .FirstOrDefaultAsync(u =>
                u.RefreshTokens.Any(rt => rt.RefreshBody == refreshToken));
        if (user is null)
            return ApplicationExecuteResult<AuthTokensPair>
                .Failure(UserErrorHelper
                    .ErrorUserNotFoundWarning($"refresh token - {refreshToken}")
                    .ToCritical(HttpStatusCode.NotFound));
        
        var rolesResult = await roleService.GetRolesByUser(user);
        if (rolesResult.IsSuccess is false)
            return ApplicationExecuteResult<AuthTokensPair>.Failure().Merge(rolesResult);
        var roles = rolesResult.Value!;
        
        var pairResult = await authTokenService.RefreshAuthTokensPairAsync(refreshToken, user, roles);
        if (pairResult.IsSuccess is false)
            return ApplicationExecuteResult<AuthTokensPair>.Failure().Merge(pairResult);
        var pair = pairResult.Value!;
        
        return ApplicationExecuteResult<AuthTokensPair>.Success(pair);
    }

    public async Task<ApplicationExecuteResult<Unit>> LogoutUserAsync(ApplicationUserEntity? user, bool globally, string? reason = null)
    {
        string? jti = null;

        if (!globally)
        {
            jti = contextAccessor.HttpContext?
                .User
                .FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(jti))
            {
                return ApplicationExecuteResult<Unit>.Failure(
                    new ApplicationError(
                        AccessTokenErrors.UnknownError,
                        "Не удалось выйти из системы",
                        "В access-токене отсутствует claim JTI",
                        ErrorSeverity.Critical,
                        HttpStatusCode.BadRequest));
            }
        }
        
        var revokeAccessRes = await authTokenService.RevokeAccessTokenAsync(user, jti, reason, globally);
        if (!revokeAccessRes.IsSuccess)
            return revokeAccessRes;

        var revokeRefreshRes = await authTokenService.BlockRefreshTokenAsync(user, reason, globally);
        if (!revokeRefreshRes.IsSuccess)
            return revokeRefreshRes;
        
        return ApplicationExecuteResult<Unit>.Success(Unit.Value);
    }
}