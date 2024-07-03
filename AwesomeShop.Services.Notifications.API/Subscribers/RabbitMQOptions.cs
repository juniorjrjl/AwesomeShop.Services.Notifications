namespace AwesomeShop.Services.Notifications.API.Subscribers;

public record RabbitMQOptions(string User, string Password, string Host, string VirtualHost,int Port);