using AwesomeShop.Services.Notifications.API;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
builder.Services.AddMongo();
builder.Services.AddRepositories();
builder.Services.AddMailService(configuration);
builder.Services.AddSubscribers();


var app = builder.Build();


app.Run();