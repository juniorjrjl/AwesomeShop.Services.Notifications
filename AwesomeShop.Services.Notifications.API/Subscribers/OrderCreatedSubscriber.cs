using System.Text;
using AwesomeShop.Services.Notifications.API.Infrastructure.Persistence.Repositories;
using AwesomeShop.Services.Notifications.API.Infrastructure.Services;
using AwesomeShop.Services.Notifications.API.Subscribers.Events;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AwesomeShop.Services.Notifications.API.Subscribers;

public class OrderCreatedSubscriber : BackgroundService
{
    
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string Queue = "notification-service/order-created";
    private const string Exchange = "notification-service";
    public OrderCreatedSubscriber(IServiceProvider serviceProvider, RabbitMQOptions rabbitMQOptions)
    {
        _serviceProvider = serviceProvider;

        var connectionFactory = new ConnectionFactory {
            HostName = rabbitMQOptions.Host,
            UserName = rabbitMQOptions.User,
            Password = rabbitMQOptions.Password,
            Port = rabbitMQOptions.Port,
            VirtualHost = rabbitMQOptions.VirtualHost
        };

        _connection = connectionFactory.CreateConnection("notifications-service-order-created-consumer"); 

        _channel = _connection.CreateModel();
        
        _channel.ExchangeDeclare(Exchange, ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(Queue, durable: false, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueBind(Queue, Exchange, Queue);
        _channel.QueueBind(Queue, "order-service", "order-created");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (sender, eventArgs) => {
                var contentArray = eventArgs.Body.ToArray();
                var contentString = Encoding.UTF8.GetString(contentArray);
                var message = JsonConvert.DeserializeObject<OrderCreated>(contentString);

                Console.WriteLine($"Message OrderCreated received {message}");

                //await SendEmail(message);

                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };

            _channel.BasicConsume(Queue, false, consumer);
                        
            return Task.CompletedTask;
        }

    /*private async Task<bool> SendEmail(OrderCreated order) 
    {
        using var scope = _serviceProvider.CreateScope();
        var emailService = scope.ServiceProvider.GetService<INotificationService>();
        ArgumentNullException.ThrowIfNull(emailService);
        var mailRepository = scope.ServiceProvider.GetService<IMailRepository>();
        ArgumentNullException.ThrowIfNull(mailRepository);

        var template = await mailRepository.GetTemplate("OrderCreated");

        var subject = string.Format(template.Subject, order.FullName);
        var content = string.Format(template.Content, order.FullName, order.Id);

        await emailService.SendAsync(subject, content, order.Email, order.FullName);

        return true;
    }*/
}
