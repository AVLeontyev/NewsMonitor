namespace NewsMonitor.Shared.Models;

public class UserTopic
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Guid TopicId { get; set; }
    public Topic Topic { get; set; } = null!;
    
    public DateTime SubscribedAt { get; set; }
}