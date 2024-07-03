using AwesomeShop.Services.Notifications.API.Infrastructure.DTOs;

namespace AwesomeShop.Services.Notifications.API.Infrastructure.Persistence.Repositories;

public interface IMailRepository
{
    
    Task<EmailTemplateDTO> GetTemplate(string @event);

    Task AddManyAsync(List<EmailTemplateDTO> templates);

}