using Microsoft.AspNetCore.SignalR;
using NewsMonitor.API.Hubs;

namespace NewsMonitor.API.Services;

public class RealTimeNotificationService
{
    private readonly IHubContext<NewsHub> _hubContext;
    private readonly ILogger<RealTimeNotificationService> _logger;

    public RealTimeNotificationService(
        IHubContext<NewsHub> hubContext,
        ILogger<RealTimeNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendAsync(string topic, string title, string? description, string sourceUrl)
    {
        await _hubContext.Clients.Group(topic).SendAsync("ReceiveNews", new
        {
            topic = topic,
            title = title,
            description = description,
            sourceUrl = sourceUrl,
            timestamp = DateTime.UtcNow
        });
        
        _logger.LogInformation($"Real-time notification sent to topic '{topic}': {title}");
    }
}