using Microsoft.AspNetCore.SignalR;
using NewsMonitor.API.Hubs;

namespace NewsMonitor.API.Services;

public class NotificationService
{
    private readonly IHubContext<NewsHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IHubContext<NewsHub> hubContext, ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyNewNewsAsync(string topic, string title, string? description, string sourceUrl)
    {
        try
        {
            var message = new
            {
                Topic = topic,
                Title = title,
                Description = description,
                SourceUrl = sourceUrl,
                Timestamp = DateTime.UtcNow
            };

            // Отправляем всем
            await _hubContext.Clients.All.SendAsync("ReceiveNews", message);
            _logger.LogInformation($"Sent notification to all clients: {title}");

            // Только подписанным 
            await _hubContext.Clients.Group(topic).SendAsync("ReceiveTopicNews", message);
            _logger.LogInformation($"Sent notification to topic group '{topic}': {title}");

            // Важное
            if (title.Contains("release") || title.Contains("announces") || title.Contains("launch"))
            {
                await _hubContext.Clients.All.SendAsync("ReceiveImportantNews", message);
                _logger.LogInformation($"Sent important notification: {title}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending notification: {title}");
        }
    }
}