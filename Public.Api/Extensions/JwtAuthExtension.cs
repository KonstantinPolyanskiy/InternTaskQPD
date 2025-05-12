using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Public.UseCase.UseCases.UserUseCases.Events;
using Settings.Common;

namespace Public.Api.Extensions;

public static class JwtAuthExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<JwtSettings>(config.GetSection("JwtSettings"));

        var jwtSettings = config.GetSection("JwtSettings").Get<JwtSettings>()
                          ?? throw new InvalidOperationException("Настройки jwt токена не найдены");
        var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

        services.AddScoped<JwtLogoutEvent>();

        services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.SaveToken = true;
                o.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero
                };

                o.EventsType = typeof(JwtLogoutEvent);
            });

        services.AddAuthorization();
        return services;
    }
}