using MassTransit;
using Microsoft.Extensions.Logging;
using NewsMonitor.Shared.Messages;
using System.Text;
using System.Text.Json;

namespace NewsMonitor.Consumer.Consumers;

public class NewsCreatedConsumer : IConsumer<NewsCreatedEvent>
{
    private readonly ILogger<NewsCreatedConsumer> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;

    public NewsCreatedConsumer(ILogger<NewsCreatedConsumer> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
        _apiBaseUrl = "http://localhost:5269"; // URL API
    }

    public async Task Consume(ConsumeContext<NewsCreatedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation($"Received news: {message.Title}");
        _logger.LogInformation($"Topic: {message.Topic}");
        _logger.LogInformation($"NewsId: {message.NewsId}");
        _logger.LogInformation($"PublishedAt: {message.PublishedAt}");

        try
        {
            // Если важно
            bool isImportant = IsImportantNews(message.Title, message.Topic);
            _logger.LogInformation($"   IsImportant: {isImportant}");

            await SendNotificationToApiAsync(message, isImportant);

            // await SaveNewsToDatabaseAsync(message); // лишнее?

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Error processing news: {message.Title}");
            throw;
        }
    }

    private bool IsImportantNews(string title, string topic)
    {
        var importantKeywords = new[]
        {
            "release", "announces", "launch", "announcement",
            "major", "breaking", "update", "new",
            "important", "critical", "urgent", 
            "релиз", "анонс", "запуск", "объявление",
            "крупный", "прорывной", "обновление", "новый",
            "важный", "критический", "срочный"
        };

        var text = $"{title} {topic}".ToLowerInvariant();
        
        foreach (var keyword in importantKeywords)
        {
            if (text.Contains(keyword))
                return true;
        }

        return false;
    }

    private async Task SendNotificationToApiAsync(NewsCreatedEvent newsEvent, bool isImportant)
    {
        try
        {
            var notification = new
            {
                Topic = newsEvent.Topic,
                Title = newsEvent.Title,
                Description = $"Новость по теме: {newsEvent.Topic}",
                SourceUrl = $"http://example.com/news/{newsEvent.NewsId}",
                IsImportant = isImportant,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(notification);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation($"Sending notification to API: {newsEvent.Title}");

            // Запрос в API
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/notifications", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Notification sent successfully: {newsEvent.Title}");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Notification failed: {response.StatusCode}, {errorContent}");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"HTTP error sending notification: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error sending notification: {ex.Message}");
        }
    }

    // Сохранение в БД (зачем?)
    private async Task SaveNewsToDatabaseAsync(NewsCreatedEvent newsEvent)
    {
        _logger.LogInformation($"Saving news to database: {newsEvent.Title}");
        
        try
        {
            var saveRequest = new
            {
                Id = newsEvent.NewsId,
                Title = newsEvent.Title,
                Topic = newsEvent.Topic,
                PublishedAt = newsEvent.PublishedAt
            };

            var json = JsonSerializer.Serialize(saveRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/news", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"News saved to database: {newsEvent.Title}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error saving news to database: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}