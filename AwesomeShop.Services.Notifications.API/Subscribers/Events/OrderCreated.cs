namespace AwesomeShop.Services.Notifications.API.Subscribers.Events;

public record OrderCreated(Guid Id, string FullName, string Email);
