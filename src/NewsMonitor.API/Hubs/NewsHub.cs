using Microsoft.AspNetCore.SignalR;

namespace NewsMonitor.API.Hubs;

public class NewsHub : Hub
{
    private readonly ILogger<NewsHub> _logger;

    public NewsHub(ILogger<NewsHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client connected: {Context.ConnectionId}");
        await Clients.Caller.SendAsync("Connected", $"Welcome! Your connection ID: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    // Подписка
    public async Task SubscribeToTopic(string topicName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, topicName);
        _logger.LogInformation($"Client {Context.ConnectionId} subscribed to topic: {topicName}");
        await Clients.Caller.SendAsync("Subscribed", $"Subscribed to topic: {topicName}");
    }

    // Отписка
    public async Task UnsubscribeFromTopic(string topicName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, topicName);
        _logger.LogInformation($"Client {Context.ConnectionId} unsubscribed from topic: {topicName}");
        await Clients.Caller.SendAsync("Unsubscribed", $"Unsubscribed from topic: {topicName}");
    }

    // Отправка уведомлений
    public async Task SendNewsNotification(string topic, string title, string message)
    {
        await Clients.Group(topic).SendAsync("ReceiveNewsNotification", new
        {
            Topic = topic,
            Title = title,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }
}