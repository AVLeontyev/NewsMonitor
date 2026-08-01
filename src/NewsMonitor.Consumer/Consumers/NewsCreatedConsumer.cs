using MassTransit;
using Microsoft.Extensions.Logging;
using NewsMonitor.Shared.Messages;

namespace NewsMonitor.Consumer.Consumers;

public class NewsCreatedConsumer : IConsumer<NewsCreatedEvent>
{
    private readonly ILogger<NewsCreatedConsumer> _logger;

    public NewsCreatedConsumer(ILogger<NewsCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<NewsCreatedEvent> context)
    {
        _logger.LogInformation($"📨 Received news: {context.Message.Title}");
        _logger.LogInformation($"   Topic: {context.Message.Topic}");
        _logger.LogInformation($"   NewsId: {context.Message.NewsId}");
        
        // Здесь будет логика обработки новости
        
        await Task.CompletedTask;
    }
}