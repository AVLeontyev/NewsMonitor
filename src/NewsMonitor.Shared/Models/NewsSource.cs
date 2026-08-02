namespace NewsMonitor.Shared.Models;

public class NewsSource
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = "RSS"; // RSS, API, HTML
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastParsedAt { get; set; }
}