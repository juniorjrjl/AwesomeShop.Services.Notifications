using AwesomeShop.Services.Notifications.API.Infrastructure.DTOs;
using AwesomeShop.Services.Notifications.API.Infrastructure.Persistence;
using AwesomeShop.Services.Notifications.API.Infrastructure.Persistence.Repositories;
using AwesomeShop.Services.Notifications.API.Infrastructure.Services;
using AwesomeShop.Services.Notifications.API.Subscribers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace AwesomeShop.Services.Notifications.API;

public static class Extensions
{
    
    public static IServiceCollection AddRepositories(this IServiceCollection services) {
        services.AddScoped<IMailRepository,MailRepository>();

        return services;
    }

    public static IServiceCollection AddMongo(this IServiceCollection services) {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        #pragma warning disable CS0618
        BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;
        BsonDefaults.GuidRepresentationMode = GuidRepresentationMode.V3;
        #pragma warning restore CS0618
        services.AddSingleton(sp => {
            var configuration = sp.GetService<IConfiguration>();
            ArgumentNullException.ThrowIfNull(configuration);

            var mongoConfig = configuration.GetSection("Mongo").Get<MongoDBOptions>();
            ArgumentNullException.ThrowIfNull(mongoConfig);

            return mongoConfig;
        });

        services.AddSingleton<IMongoClient>(sp => {
            var options = sp.GetService<MongoDBOptions>();
            ArgumentNullException.ThrowIfNull(options);
            
            return new MongoClient(options.ConnectionStrings);
        });

        services.AddTransient(sp => {
            
            var options = sp.GetService<MongoDBOptions>();
            ArgumentNullException.ThrowIfNull(options);
            var mongoClient = sp.GetService<IMongoClient>();
            ArgumentNullException.ThrowIfNull(mongoClient);

            return mongoClient.GetDatabase(options.Database);
        });

        return services;
    }

    public static IServiceCollection AddMailService(this IServiceCollection services, IConfiguration configuration) {
        services.AddSingleton(sp => {
            var configuration = sp.GetService<IConfiguration>();
            ArgumentNullException.ThrowIfNull(configuration);

            var mailConfig = configuration.GetSection("Notifications").Get<MailConfig>();
            ArgumentNullException.ThrowIfNull(mailConfig);

            return mailConfig;
        });

        var mailConfig = configuration.GetSection("Notifications").Get<MailConfig>();
        ArgumentNullException.ThrowIfNull(mailConfig);

        services.AddTransient<INotificationService, NotificationService>();

        return services;
    }

    public static IServiceCollection AddSubscribers(this IServiceCollection services) {
        services.AddSingleton(s => {
            var configuration = s.GetService<IConfiguration>();
            ArgumentNullException.ThrowIfNull(configuration);

            var rabbitMQConfig = configuration.GetSection("RabbitMQ").Get<RabbitMQOptions>();
            ArgumentNullException.ThrowIfNull(rabbitMQConfig);

            return rabbitMQConfig;
        });
        services.AddHostedService<CustomerCreatedSubscriber>();
        services.AddHostedService<OrderCreatedSubscriber>();
        services.AddHostedService<PaymentAcceptedSubscriber>();

        return services;
    }

    public async static void Seed(IMailRepository repository)
    {
        var templates = new List<EmailTemplateDTO>();
        var orderCreated = await repository.GetTemplate("OrderCreated");
        if (orderCreated is null)
        {
            templates.Add(new EmailTemplateDTO(
                Guid.NewGuid(), 
                "Your payment is confirmed!", 
                "Hi, Your payment for the order ID {0} is confirmed. Your product will be prepared and sent soon.",
                "PaymentAccepted"
            ));
        }
        var customerCreated = await repository.GetTemplate("CustomerCreated");
        if (customerCreated is null)
        {
            templates.Add(new EmailTemplateDTO(
                Guid.NewGuid(),
                "Welcome, {0}!",
                "Welcome to AwesomeShop, {0}! You can search our products in awesome-shop-dot-com,",
                "CustomerCreated"
            ));
        }
        var paymentAccepted = await repository.GetTemplate("PaymentAccepted");
        if (paymentAccepted is null)
        {
            templates.Add(new EmailTemplateDTO(
                Guid.NewGuid(),
                "Your order is confirmed, {0}!",
                "Hi, {0}. Your order with ID {1} is confirmed. Your payment will be confirmed soon.",
                "OrderCreated"
            ));
        }
        if (templates.Count != 0)
        {
            await repository.AddManyAsync(templates);
        }
    }

}