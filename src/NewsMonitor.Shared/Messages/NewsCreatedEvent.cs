namespace NewsMonitor.Shared.Messages;

public record NewsCreatedEvent
{
    public Guid NewsId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Topic { get; init; } = string.Empty;
    public DateTime PublishedAt { get; init; }
}