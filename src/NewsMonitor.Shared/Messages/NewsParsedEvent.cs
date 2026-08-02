namespace NewsMonitor.Shared.Messages;

public record NewsParsedEvent
{
    public Guid NewsId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string SourceUrl { get; init; } = string.Empty;
    public string? SourceName { get; init; }
    public DateTime PublishedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public string Topic { get; init; } = string.Empty;
}