using QPDCar.Infrastructure.Mail;
using QPDCar.Models.ApplicationModels.Settings;
using QPDCar.Notifications.Consumers;
using QPDCar.ServiceInterfaces.MailServices;
using RabbitMQ.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));

builder.Services.AddSingleton<IConnection>(sp =>
{
    var cfg     = sp.GetRequiredService<IConfiguration>();
    var factory = new ConnectionFactory
    {
        HostName = cfg["Rabbit:Host"],
        UserName = cfg["Rabbit:User"],
        Password = cfg["Rabbit:Pass"],
    };

    return factory.CreateConnectionAsync("api-publisher").Result;
});

builder.Services.AddTransient<IMailSender, MailSmtpSender>();
builder.Services.AddHostedService<RabbitEmailConsumer>();

await builder.Build().RunAsync();