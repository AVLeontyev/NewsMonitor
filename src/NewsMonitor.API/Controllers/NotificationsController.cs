// NewsMonitor.API/Controllers/NotificationsController.cs
using Microsoft.AspNetCore.Mvc;
using NewsMonitor.API.Services;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly RealTimeNotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        RealTimeNotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] NotificationRequest request)
    {
        _logger.LogInformation($"Received notification for topic: {request.Topic}");
        
        await _notificationService.SendAsync(
            request.Topic,
            request.Title,
            request.Description,
            request.SourceUrl);

        return Ok();
    }
}

public class NotificationRequest
{
    public string Topic { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
}