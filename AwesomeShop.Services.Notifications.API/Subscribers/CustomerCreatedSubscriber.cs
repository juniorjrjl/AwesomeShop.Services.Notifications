
using System.Text;
using AwesomeShop.Services.Notifications.API.Infrastructure.Persistence.Repositories;
using AwesomeShop.Services.Notifications.API.Infrastructure.Services;
using AwesomeShop.Services.Notifications.API.Subscribers.Events;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AwesomeShop.Services.Notifications.API.Subscribers;

public class CustomerCreatedSubscriber : BackgroundService
{

    private readonly IServiceProvider _serviceProvider;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string Queue = "notification-service/customer-created";
    private const string Exchange = "notification-service";


    public CustomerCreatedSubscriber(IServiceProvider serviceProvider, RabbitMQOptions rabbitMQOptions)
    {
        _serviceProvider = serviceProvider;

        var connectionFactory = new ConnectionFactory {
            HostName = rabbitMQOptions.Host,
            UserName = rabbitMQOptions.User,
            Password = rabbitMQOptions.Password,
            Port = rabbitMQOptions.Port,
            VirtualHost = rabbitMQOptions.VirtualHost
        };

        _connection = connectionFactory.CreateConnection("notifications-service-customer-created-consumer"); 

        _channel = _connection.CreateModel();
        
        _channel.ExchangeDeclare(Exchange, ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(Queue, durable: false, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueBind(Queue, Exchange, Queue);
        _channel.QueueBind(Queue, "customer-service", "customer-created");
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (sender, eventArgs) => 
        {
            var contentArray = eventArgs.Body.ToArray();
            var contentString = Encoding.UTF8.GetString(contentArray);
            var message = JsonConvert.DeserializeObject<CustomerCreated>(contentString);
            ArgumentNullException.ThrowIfNull(message);

            Console.WriteLine($"Message CustomerCreated received {message}");

            await SendEmail(message);

            _channel.BasicAck(eventArgs.DeliveryTag, false);
        };

        _channel.BasicConsume(Queue, false, consumer);
    }

    private async Task<bool> SendEmail(CustomerCreated customer)
    {
        using var scope = _serviceProvider.CreateScope();
        var emailService = scope.ServiceProvider.GetService<INotificationService>();
        ArgumentNullException.ThrowIfNull(emailService);
        var mailRepository = scope.ServiceProvider.GetService<IMailRepository>();
        ArgumentNullException.ThrowIfNull(mailRepository);

        var template = await mailRepository.GetTemplate("CustomerCreated");

        var subject = string.Format(template.Subject, customer.FullName);
        var content = string.Format(template.Content, customer.FullName);

        await emailService.SendAsync(subject, content, customer.Email, customer.FullName);

        return true;
    }

}