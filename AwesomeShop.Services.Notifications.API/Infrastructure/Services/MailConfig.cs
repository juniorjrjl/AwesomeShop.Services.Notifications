namespace AwesomeShop.Services.Notifications.API.Infrastructure.Services;

public record MailConfig(string FromName, string FromEmail, string Password, string SmtpHost, int SmtpPort);
