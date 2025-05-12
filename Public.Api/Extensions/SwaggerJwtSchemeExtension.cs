using Microsoft.OpenApi.Models;

namespace Public.Api.Extensions;

public static class SwaggerJwtScheme
{
    public static IServiceCollection AddSwaggerJwtScheme(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            var bearerScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                },
                Scheme       = "bearer",
                Name         = "Authorization",
                In           = ParameterLocation.Header,
                Type         = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Description  = "Вставьте ТОЛЬКО access-токен без префикса «Bearer »"
            };

            c.AddSecurityDefinition("Bearer", bearerScheme);

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [ bearerScheme ] = Array.Empty<string>()
            });
        });
        
        return services;
    }
}