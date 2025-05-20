using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QPDCar.Infrastructure.Mail;
using QPDCar.Models.ApplicationModels.Settings;
using QPDCar.ServiceInterfaces.MailServices;

namespace QPDCar.Infrastructure.Extensions;

public static class MailDependencyInjection
{
    /// <summary> Добавляет сервис отправки Email </summary>
    public static IServiceCollection AddMailSender(
        this IServiceCollection services,
        IConfiguration configSection)       
    {
        services.Configure<SmtpSettings>(configSection);
        services.AddTransient<IMailSender, MailSmtpSender>();
        
        return services;
    }
}