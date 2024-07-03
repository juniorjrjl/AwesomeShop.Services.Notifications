namespace AwesomeShop.Services.Notifications.API.Subscribers.Events;

public record CustomerCreated(Guid Id, string FullName, string Email);
