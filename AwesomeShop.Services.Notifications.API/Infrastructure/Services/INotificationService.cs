namespace AwesomeShop.Services.Notifications.API.Infrastructure.Services;

public interface INotificationService
{
    
    Task SendAsync(string subject, string content, string toEmail, string toName);

}
