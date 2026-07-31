namespace NewsMonitor.Shared.Models;

public class News
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
    public string? SourceName { get; set; }
    public DateTime PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string? Hash { get; set; } // Для предотвращения дубликатов
    public bool IsImportant { get; set; }
}