using System.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;
using QPDCar.Models.ApplicationModels.ErrorTypes;
using QPDCar.Models.ApplicationModels.Settings;
using QPDCar.ServiceInterfaces.MailServices;

namespace QPDCar.Infrastructure.Mail;

/// <summary> Сервис отправки Email сообщений по протоколу SMTP </summary> 
public class MailSmtpSender(IOptions<SmtpSettings> opts, ILogger<MailSmtpSender> logger) : IMailSender
{
    public async Task<ApplicationExecuteResult<Unit>> SendAsync(string to, string subject, string body)
    {
        try
        {
            var msg = BuildMessage(to, subject, body);

            using var client = new SmtpClient();

            await client.ConnectAsync(
                opts.Value.Host,
                opts.Value.Port,
                opts.Value.UseSsl ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTlsWhenAvailable);

            await client.AuthenticateAsync(opts.Value.UserName, opts.Value.Password);
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);

            logger.LogInformation("E-mail “{Subject}” успешно отправлен на {To}", subject, to);
            
            return ApplicationExecuteResult<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Не удалось отправить e-mail “{Subject}” на {To}: {Message}", subject, to, ex.Message);

            return ApplicationExecuteResult<Unit>.Failure( new ApplicationError(
                EmailErrors.MailNotSend, "Письмо не отправлено",
                $"Не удалось отправить письмо на почту {to}",
                ErrorSeverity.Critical, HttpStatusCode.InternalServerError)); 
        }
    }
    private MimeMessage BuildMessage(string to, string subject, string bodyHtml)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(opts.Value.FromName ?? opts.Value.FromAddress, opts.Value.FromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var body = new BodyBuilder { HtmlBody = bodyHtml };
        message.Body = body.ToMessageBody();

        return message;
    }
}