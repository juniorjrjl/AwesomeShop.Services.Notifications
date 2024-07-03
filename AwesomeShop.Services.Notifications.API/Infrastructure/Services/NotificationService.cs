
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AwesomeShop.Services.Notifications.API.Infrastructure.Services;

public class NotificationService(MailConfig config, ISendGridClient client) : INotificationService
{
    private readonly MailConfig _config = config;

    private readonly ISendGridClient _client = client;

    public async Task SendAsync(string subject, string content, string toEmail, string toName)
    {
        var from = new EmailAddress(_config.FromEmail, _config.FromName);
        var to = new EmailAddress(toEmail, toName);
        
        var message = new SendGridMessage{
            From = from,
            Subject = subject,
        };

        message.AddContent(MimeType.Html, content);
        message.AddTo(to);

        message.SetClickTracking(false, false);
        message.SetOpenTracking(false);
        message.SetGoogleAnalytics(false);
        message.SetSubscriptionTracking(false);
        
        await _client.SendEmailAsync(message);
    }
}