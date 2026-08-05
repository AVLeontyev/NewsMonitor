using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsMonitor.Shared.Data;
using NewsMonitor.Shared.Models;

namespace NewsMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NewsController> _logger;

    public NewsController(ApplicationDbContext context, ILogger<NewsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? topic = null, [FromQuery] bool? important = null)
    {
        var query = _context.News.AsQueryable();

        if (!string.IsNullOrEmpty(topic))
        {
            query = query.Where(n => n.Topic.Contains(topic));
        }

        if (important.HasValue && important.Value)
        {
            query = query.Where(n => n.IsImportant);
        }

        var news = await query
            .OrderByDescending(n => n.PublishedAt)
            .Take(100)
            .ToListAsync();

        return Ok(news);
    }

    [HttpGet("topic/{topicName}")]
    public async Task<IActionResult> GetByTopic(string topicName)
    {
        var news = await _context.News
            .Where(n => n.Topic == topicName)
            .OrderByDescending(n => n.PublishedAt)
            .Take(50)
            .ToListAsync();

        return Ok(news);
    }

    [HttpGet("important")]
    public async Task<IActionResult> GetImportant()
    {
        var news = await _context.News
            .Where(n => n.IsImportant)
            .OrderByDescending(n => n.PublishedAt)
            .Take(50)
            .ToListAsync();

        return Ok(news);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var news = await _context.News.FindAsync(id);
        if (news == null)
            return NotFound();

        return Ok(news);
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var news = await _context.News.FindAsync(id);
        if (news == null)
            return NotFound();

        // прочтение/метка. ?

        return Ok(new { success = true });
    }
}