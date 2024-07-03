using System.Text;
using AwesomeShop.Services.Notifications.API.Infrastructure.Persistence.Repositories;
using AwesomeShop.Services.Notifications.API.Infrastructure.Services;
using AwesomeShop.Services.Notifications.API.Subscribers.Events;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AwesomeShop.Services.Notifications.API.Subscribers;

public class PaymentAcceptedSubscriber : BackgroundService
{
    
        private readonly IServiceProvider _serviceProvider;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string Queue = "notification-service/payment-accepted";
        private const string Exchange = "notification-service";
        private const string RoutingKey = "payment-accepted";
        
        public PaymentAcceptedSubscriber(IServiceProvider serviceProvider, RabbitMQOptions rabbitMQOptions)
        {
            _serviceProvider = serviceProvider;

        var connectionFactory = new ConnectionFactory {
            HostName = rabbitMQOptions.Host,
            UserName = rabbitMQOptions.User,
            Password = rabbitMQOptions.Password,
            Port = rabbitMQOptions.Port,
            VirtualHost = rabbitMQOptions.VirtualHost
        };

            _connection = connectionFactory.CreateConnection("notifications-service-payment-accepted-consumer"); 

            _channel = _connection.CreateModel();
            
            _channel.ExchangeDeclare(Exchange, ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(Queue, durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(Queue, Exchange, RoutingKey);
            _channel.QueueBind(Queue, "payment-service", RoutingKey);
        }
        
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (sender, eventArgs) => {
            var contentArray = eventArgs.Body.ToArray();
            var contentString = Encoding.UTF8.GetString(contentArray);
            var message = JsonConvert.DeserializeObject<PaymentAccepted>(contentString);
            ArgumentNullException.ThrowIfNull(message);

            Console.WriteLine($"[notification-service] Message PaymentAccepted received {message}");

            await SendEmail(message);

            _channel.BasicAck(eventArgs.DeliveryTag, false);
        };

        _channel.BasicConsume(Queue, false, consumer);
    }

    private async Task<bool> SendEmail(PaymentAccepted payment) {
        using var scope = _serviceProvider.CreateScope();
        var emailService = scope.ServiceProvider.GetService<INotificationService>();
        ArgumentNullException.ThrowIfNull(emailService);
        var mailRepository = scope.ServiceProvider.GetService<IMailRepository>();
        ArgumentNullException.ThrowIfNull(mailRepository);

        var template = await mailRepository.GetTemplate("PaymentAccepted");

        var subject = template.Subject;
        var content = string.Format(template.Content, payment.Id);

        await emailService.SendAsync(subject, content, payment.Email, payment.FullName);

        return true;
    }
}
