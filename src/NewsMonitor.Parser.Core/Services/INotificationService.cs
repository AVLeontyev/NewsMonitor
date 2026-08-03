namespace NewsMonitor.Parser.Core.Services;

public interface INotificationService
{
    Task NotifyNewNewsAsync(string topic, string title, string? description, string sourceUrl);
}