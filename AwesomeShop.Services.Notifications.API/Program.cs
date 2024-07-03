using AwesomeShop.Services.Notifications.API;
using AwesomeShop.Services.Notifications.API.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
builder.Services.AddMongo();
builder.Services.AddRepositories();
builder.Services.AddMailService(configuration);
builder.Services.AddSubscribers();


var app = builder.Build();

using (var scope = app.Services.CreateScope()){
    var repository = scope.ServiceProvider.GetRequiredService<IMailRepository>();
    Extensions.Seed(repository);
}

app.Run();