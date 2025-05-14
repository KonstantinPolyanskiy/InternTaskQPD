using System.Net;
using Microsoft.AspNetCore.Identity;
using Private.Services.ErrorHelpers;
using Private.Services.Repositories;
using Private.ServicesInterfaces;
using Private.ServicesInterfaces.Commands.AuthTokenServiceCommands;
using Private.StorageModels;
using Public.Models.ApplicationErrors;
using Public.Models.BusinessModels.TokenModels;
using Public.Models.CommonModels;
using Public.Models.Extensions;

namespace Private.Services.AuthServices;

public class IdentityAuthService(SignInManager<ApplicationUserEntity> signInManager, IUserService userService,
    IAuthTokenService authTokenService, IRoleService roleService, IRefreshTokenRepository refreshRepository) : IAuthService
{
    private const string AuthPairObjectName = "Auth token pair";
    public async Task<ApplicationExecuteLogicResult<AuthTokensPair>> SignInAndGetAuthTokensAsync(string login, string password)
    {
        var signInResult = await signInManager.PasswordSignInAsync(login, password, false, false);
        if (signInResult.Succeeded is false)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure(new ApplicationError(
                UserErrors.InvalidLoginOrPassword, "Не верный логин или пароль",
                "Не получилось авторизовать пользователя по переданному логину/парою",
                ErrorSeverity.Critical, HttpStatusCode.Unauthorized));
        
        var userResult = await userService.ByLoginOrIdAsync(login);
        if (userResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure(new ApplicationError(
                UserErrors.UserNotFound, "Не получилось найти пользователя",
                $"Авторизация прошла успешно, но пользователь {login} не найден",
                ErrorSeverity.Critical, HttpStatusCode.InternalServerError));
        var user = userResult.Value!;
        
        var rolesResult = await roleService.GetRolesByUser(user);
        if (rolesResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure().Merge(rolesResult);
        var roles = rolesResult.Value!;

        var tokensResult = await authTokenService.GenerateAuthTokensPairAsync(user, roles);
        if (tokensResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure().Merge(tokensResult);
        var tokens = tokensResult.Value!;
        
        return ApplicationExecuteLogicResult<AuthTokensPair>.Success(tokens);
    }

    public async Task<ApplicationExecuteLogicResult<AuthTokensPair>> GetFreshTokensAsync(string refreshToken)
    {
        var oldTokenResult = await refreshRepository.TokenByBodyAsync(refreshToken);
        if (oldTokenResult.IsSuccess is false)
            return ErrorHelper.WrapAllDbErrors<RefreshTokenEntity, AuthTokensPair>(RefreshTokenErrors.TokenNotFound, oldTokenResult, string.Join(" ", AuthPairObjectName, refreshToken));
        var oldToken = oldTokenResult.Value!;
        
        var userResult = await userService.ByLoginOrIdAsync(oldToken.UserId);
        if (userResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure(new ApplicationError(
                UserErrors.UserNotFound, "Не получилось найти пользователя",
                $"Refresh токен с таким телом существует, но пользователь с refresh {refreshToken} не найден",
                ErrorSeverity.Critical, HttpStatusCode.InternalServerError));
        var user = userResult.Value!;
        
        var rolesResult = await roleService.GetRolesByUser(user);
        if (rolesResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure().Merge(rolesResult);
        var roles = rolesResult.Value!;
        
        var tokensResult = await authTokenService.RefreshAuthTokensPairAsync(refreshToken, user, roles);
        if (tokensResult.IsSuccess is false)
            return ApplicationExecuteLogicResult<AuthTokensPair>.Failure().Merge(tokensResult);
        var tokens = tokensResult.Value!;
        
        return ApplicationExecuteLogicResult<AuthTokensPair>.Success(tokens);
    }

    public async Task<ApplicationExecuteLogicResult<Unit>> LogoutUserAsync(ApplicationUserEntity? user, bool globally, string? reason = null)
    { 
        var tokenResult = await refreshRepository.TokenByUserId(user!.Id);
        if (tokenResult.IsSuccess is false)
            return ErrorHelper.WrapDbExceptionError<RefreshTokenEntity, Unit>(AccessTokenErrors.UnknownError, tokenResult, string.Join(" ", "Refresh token", user.UserName));
        var token = tokenResult.Value!;
        
        var accessRevoked = await authTokenService.RevokeAccessTokenAsync(new BlockAccessTokenCommand
        {
            AccessJti = token.AccessJti,
            User = user,
            LogoutAll = globally,
            Reason = reason
        });

        var refreshRevoked = await authTokenService.BlockRefreshTokenAsync(new BlockRefreshTokenCommand
        {
            User = user,
            LogoutAll = globally
        });

        return ApplicationExecuteLogicResult<Unit>.Success(Unit.Value)
            .WithCriticals(accessRevoked.GetCriticalErrors)
            .WithCriticals(refreshRevoked.GetCriticalErrors)
            .WithWarnings(refreshRevoked.GetWarnings)
            .WithWarnings(accessRevoked.GetWarnings);
    }
}