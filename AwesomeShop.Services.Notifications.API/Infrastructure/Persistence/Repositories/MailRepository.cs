using AwesomeShop.Services.Notifications.API.Infrastructure.DTOs;
using MongoDB.Driver;

namespace AwesomeShop.Services.Notifications.API.Infrastructure.Persistence.Repositories;

public class MailRepository(IMongoDatabase database) : IMailRepository
{

    private readonly IMongoCollection<EmailTemplateDTO> _collection = database.GetCollection<EmailTemplateDTO>("email-templates");

    public async Task<EmailTemplateDTO> GetTemplate(string @event) => 
        await _collection.Find(c => c.Event == @event).SingleOrDefaultAsync();

    public async Task AddManyAsync(List<EmailTemplateDTO> templates) =>
        await _collection.InsertManyAsync(templates);

}
