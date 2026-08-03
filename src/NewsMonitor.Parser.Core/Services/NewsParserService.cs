using System.ServiceModel.Syndication;
using System.Xml;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewsMonitor.Shared.Data;
using NewsMonitor.Shared.Messages;
using NewsMonitor.Shared.Models;
using NewsMonitor.Parser.Core.Services;

namespace NewsMonitor.Parser.Core.Services;

public class NewsParserService
{
    private readonly ApplicationDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<NewsParserService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly INotificationService _notificationService;

    public NewsParserService(
        ApplicationDbContext context,
        IPublishEndpoint publishEndpoint,
        ILogger<NewsParserService> logger,
        IHttpClientFactory httpClientFactory,
        INotificationService notificationService) 
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _notificationService = notificationService;
    }

    private HttpClient CreateHttpClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        client.Timeout = TimeSpan.FromSeconds(30);
        return client;
    }

    public async Task ParseAllTopicsAsync()
    {
        _logger.LogInformation("Starting parsing all active topics...");

        var topics = await _context.Topics
            .Where(t => t.IsActive)
            .ToListAsync();

        if (!topics.Any())
        {
            _logger.LogWarning("No active topics found for parsing");
            return;
        }

        _logger.LogInformation($"Found {topics.Count} topics to parse");

        foreach (var topic in topics)
        {
            await ParseTopicAsync(topic);
        }

        _logger.LogInformation("Finished parsing all topics");
    }

    public async Task ParseTopicAsync(Topic topic)
    {
        try
        {
            _logger.LogInformation($"Parsing topic: {topic.Name}");

            var newsItems = await FetchNewsForTopicAsync(topic);

            if (!newsItems.Any())
            {
                _logger.LogWarning($"No news found for topic: {topic.Name}");
                return;
            }

            _logger.LogInformation($"Found {newsItems.Count} news items for topic: {topic.Name}");

            foreach (var newsItem in newsItems)
            {
                await ProcessNewsItemAsync(newsItem, topic);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error parsing topic: {topic.Name}");
        }
    }

    private async Task<List<NewsItem>> FetchNewsForTopicAsync(Topic topic)
    {
        var newsItems = new List<NewsItem>();

        var sources = new[]
        {
            "https://news.ycombinator.com/rss",
            "https://feeds.feedburner.com/TechCrunch",
            "https://www.theverge.com/rss/index.xml",
            "https://www.wired.com/feed/rss",
            "https://www.theguardian.com/world/rss",
            "https://www.bbc.com/news/rss.xml"
        };

        foreach (var sourceUrl in sources)
        {
            try
            {
                var items = await ParseRssFeedAsync(sourceUrl, topic.Keywords);
                newsItems.AddRange(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error parsing RSS feed: {sourceUrl}");
            }
        }

        return newsItems;
    }

    private async Task<List<NewsItem>> ParseRssFeedAsync(string feedUrl, string? keywords)
    {
        var newsItems = new List<NewsItem>();

        try
        {
            using var client = CreateHttpClient();
            var response = await client.GetAsync(feedUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            using var reader = XmlReader.Create(new StringReader(content));
            var feed = SyndicationFeed.Load(reader);

            foreach (var item in feed.Items.Take(10))
            {
                if (!string.IsNullOrEmpty(keywords) && !IsRelevant(item, keywords))
                    continue;

                var newsItem = new NewsItem
                {
                    Title = item.Title?.Text ?? "No title",
                    Description = item.Summary?.Text ?? "",
                    SourceUrl = item.Links.FirstOrDefault()?.Uri?.ToString() ?? feedUrl,
                    SourceName = feed.Title?.Text ?? "Unknown source",
                    PublishedAt = item.PublishDate.DateTime != DateTime.MinValue 
                        ? item.PublishDate.DateTime 
                        : DateTime.UtcNow
                };

                newsItems.Add(newsItem);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error parsing RSS feed: {feedUrl}");
        }

        return newsItems;
    }

    private bool IsRelevant(SyndicationItem item, string keywords)
    {
        if (string.IsNullOrEmpty(keywords))
            return true;

        var keywordList = keywords.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.Trim().ToLower());

        var title = item.Title?.Text?.ToLower() ?? "";
        var summary = item.Summary?.Text?.ToLower() ?? "";

        foreach (var keyword in keywordList)
        {
            if (title.Contains(keyword) || summary.Contains(keyword))
                return true;
        }

        return false;
    }

    private async Task ProcessNewsItemAsync(NewsItem newsItem, Topic topic)
    {
        try
        {
            var existingNews = await _context.News
                .FirstOrDefaultAsync(n => 
                    n.Title == newsItem.Title && 
                    n.SourceUrl == newsItem.SourceUrl);

            if (existingNews != null)
            {
                _logger.LogInformation($"News already exists: {newsItem.Title}");
                return;
            }

            var hash = $"{newsItem.Title}|{newsItem.SourceUrl}".GetHashCode().ToString();

            var news = new News
            {
                Id = Guid.NewGuid(),
                Title = newsItem.Title,
                Description = newsItem.Description,
                SourceUrl = newsItem.SourceUrl,
                SourceName = newsItem.SourceName,
                PublishedAt = newsItem.PublishedAt,
                CreatedAt = DateTime.UtcNow,
                Topic = topic.Name,
                Hash = hash,
                IsImportant = IsNewsImportant(newsItem, topic)
            };

            _context.News.Add(news);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Saved news: {news.Title}");

            await _publishEndpoint.Publish(new NewsParsedEvent
            {
                NewsId = news.Id,
                Title = news.Title,
                Description = news.Description,
                SourceUrl = news.SourceUrl,
                SourceName = news.SourceName,
                PublishedAt = news.PublishedAt,
                CreatedAt = news.CreatedAt,
                Topic = news.Topic
            });

            _logger.LogInformation($"Published news event: {news.Title}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing news: {newsItem.Title}");
        }
    }

    private bool IsNewsImportant(NewsItem newsItem, Topic topic)
    {
        var importantKeywords = new[] { "release", "announces", "launch", "announcement", "major", "breaking" };
        var text = $"{newsItem.Title} {newsItem.Description}".ToLower();
        
        foreach (var keyword in importantKeywords)
        {
            if (text.Contains(keyword))
                return true;
        }
        return false;
    }
}

public class NewsItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
}