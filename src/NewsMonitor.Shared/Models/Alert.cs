namespace NewsMonitor.Shared.Models;

public class Alert
{
    public Guid Id { get; set; }
    public Guid NewsId { get; set; }
    public News News { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}