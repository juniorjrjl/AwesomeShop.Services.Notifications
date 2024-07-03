using MailKit.Net.Smtp;
using MimeKit;

namespace AwesomeShop.Services.Notifications.API.Infrastructure.Services;

public class NotificationService(MailConfig config) : INotificationService
{
    private readonly MailConfig _config = config;

    public async Task SendAsync(string subject, string content, string toEmail, string toName)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_config.FromName, _config.FromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html")
        {
            Text = content
        };

        using var client = new SmtpClient();
        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
        await client.ConnectAsync(_config.SmtpHost, _config.SmtpPort, false);
        //await client.AuthenticateAsync(_config.FromEmail, _config.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

    }
}