namespace AwesomeShop.Services.Notifications.API.Infrastructure.DTOs;

public record EmailTemplateDTO(Guid Id, string Subject, string Content, string Event);
