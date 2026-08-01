using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace NewsMonitor.API.Services;

public class RabbitMQPublisher : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName = "news_exchange";
    private readonly string _queueName = "news_queue";
    private readonly string _routingKey = "news.created";

    public RabbitMQPublisher(IConnection connection)
    {
        _connection = connection;
        _channel = _connection.CreateModel();
        
        // Declare exchange
        _channel.ExchangeDeclare(
            exchange: _exchangeName, 
            type: ExchangeType.Direct, 
            durable: true, 
            autoDelete: false);
        
        // Declare queue
        _channel.QueueDeclare(
            queue: _queueName, 
            durable: true, 
            exclusive: false, 
            autoDelete: false);
        
        // Bind queue to exchange
        _channel.QueueBind(
            queue: _queueName, 
            exchange: _exchangeName, 
            routingKey: _routingKey);
        
        Console.WriteLine($"✅ RabbitMQ configured: Exchange '{_exchangeName}', Queue '{_queueName}'");
    }

    public void PublishNewsCreatedEvent(Guid newsId, string title, string topic)
    {
        try
        {
            var message = new
            {
                NewsId = newsId,
                Title = title,
                Topic = topic,
                PublishedAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";

            _channel.BasicPublish(
                exchange: _exchangeName,
                routingKey: _routingKey,
                basicProperties: properties,
                body: body
            );
            
            Console.WriteLine($"📤 Published message: {title}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error publishing message: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}