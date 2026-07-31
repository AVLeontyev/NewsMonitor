namespace NewsMonitor.Shared.Models;

public class Topic
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Keywords { get; set; } // Поисковые ключевые слова через запятую
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    
    // Связи
    public List<UserTopic> UserTopics { get; set; } = new();
    public List<News> News { get; set; } = new();
}