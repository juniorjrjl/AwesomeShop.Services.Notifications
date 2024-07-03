using AwesomeShop.Services.Notifications.API.Infrastructure.Persistence;
using AwesomeShop.Services.Notifications.API.Infrastructure.Persistence.Repositories;
using AwesomeShop.Services.Notifications.API.Infrastructure.Services;
using AwesomeShop.Services.Notifications.API.Subscribers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using SendGrid.Extensions.DependencyInjection;

namespace AwesomeShop.Services.Notifications.API;

public static class Extensions
{
    
    public static IServiceCollection AddRepositories(this IServiceCollection services) {
        services.AddScoped<IMailRepository,MailRepository>();

        return services;
    }

    public static IServiceCollection AddMongo(this IServiceCollection services) {
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
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            
            var options = sp.GetService<MongoDBOptions>();
            ArgumentNullException.ThrowIfNull(options);
            var mongoClient = sp.GetService<IMongoClient>();
            ArgumentNullException.ThrowIfNull(mongoClient);

            return mongoClient.GetDatabase(options.Database);
        });

        return services;
    }

    public static IServiceCollection AddMailService(this IServiceCollection services, IConfiguration configuration) {
        /*services.AddSingleton(sp => {
            var configuration = sp.GetService<IConfiguration>();
            ArgumentNullException.ThrowIfNull(configuration);

            var mailConfig = configuration.GetSection("Notifications").Get<MailConfig>();
            ArgumentNullException.ThrowIfNull(mailConfig);

            return mailConfig;
        });

        var mailConfig = configuration.GetSection("Notifications").Get<MailConfig>();
        ArgumentNullException.ThrowIfNull(mailConfig);
        
        services.AddSendGrid(c => c.ApiKey = mailConfig.SendGridApiKey);

        services.AddTransient<INotificationService, NotificationService>();*/

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

}