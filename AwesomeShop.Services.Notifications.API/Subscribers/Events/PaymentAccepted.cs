namespace AwesomeShop.Services.Notifications.API.Subscribers.Events;

public record PaymentAccepted(Guid Id, string FullName, string Email);
