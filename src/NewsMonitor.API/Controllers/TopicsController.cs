using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsMonitor.Shared.Data;
using NewsMonitor.Shared.Messages;
using NewsMonitor.Shared.Models;

namespace NewsMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopicsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public TopicsController(ApplicationDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    // GET: api/topics
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var topics = await _context.Topics
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();
        
        return Ok(topics);
    }

    // GET: api/topics/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var topic = await _context.Topics
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
        
        if (topic == null)
            return NotFound();

        return Ok(topic);
    }

    // POST: api/topics
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTopicRequest request)
    {
        var topic = new Topic
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Keywords = request.Keywords,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Topics.Add(topic);
        await _context.SaveChangesAsync();

        // Отправляем событие через MassTransit
        await _publishEndpoint.Publish(new NewsCreatedEvent
        {
            NewsId = topic.Id,
            Title = $"New topic created: {topic.Name}",
            Topic = topic.Name,
            PublishedAt = DateTime.UtcNow
        });

        return CreatedAtAction(nameof(GetById), new { id = topic.Id }, topic);
    }

    // PUT: api/topics/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTopicRequest request)
    {
        var topic = await _context.Topics
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
        
        if (topic == null)
            return NotFound();

        topic.Name = request.Name;
        topic.Keywords = request.Keywords;
        
        await _context.SaveChangesAsync();

        return Ok(topic);
    }

    // DELETE: api/topics/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var topic = await _context.Topics
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
        
        if (topic == null)
            return NotFound();

        topic.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

// DTOs
public class CreateTopicRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Keywords { get; set; }
}

public class UpdateTopicRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Keywords { get; set; }
}