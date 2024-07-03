namespace AwesomeShop.Services.Notifications.API.Infrastructure.Services;

public record MailConfig(string SendGridApiKey, string FromName, string FromEmail);