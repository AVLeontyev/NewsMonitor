using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace NewsMonitor.Parser.Core.Services;

public class HttpNotificationService : INotificationService
{
    private readonly ILogger<HttpNotificationService> _logger;
    private readonly HttpClient _httpClient;

    public HttpNotificationService(ILogger<HttpNotificationService> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
        
        var apiUrl = Environment.GetEnvironmentVariable("API_URL") ?? "http://localhost:5269";
        _httpClient.BaseAddress = new Uri(apiUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
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
                SourceUrl = sourceUrl
            };

            var json = JsonSerializer.Serialize(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/notifications", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Notification sent: {title}");
            }
            else
            {
                _logger.LogWarning($"Notification failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending notification: {title}");
        }
    }
}