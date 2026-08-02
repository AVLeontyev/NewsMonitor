using Hangfire;
using Hangfire.PostgreSql;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NewsMonitor.Shared.Data;
using NewsMonitor.Shared.Messages;
using NewsMonitor.Parser.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<NewsParserService>();
builder.Services.AddHttpClient();

// Регистрация DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"=== DIAGNOSTIC ===");
Console.WriteLine($"Connection String from config: {connectionString}");
Console.WriteLine($"ASPNETCORE_ENVIRONMENT: {builder.Environment.EnvironmentName}");
Console.WriteLine($"==================");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Настройка MassTransit 7.x (с 9 вылезли проблемы с лиц.)
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri("rabbitmq://localhost/newsmonitor"), h =>
        {
            h.Username("newsuser");
            h.Password("newspassword");
        });
        
        cfg.ConfigureEndpoints(context);
    });
});

// Настройка Hangfire
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(connectionString));
builder.Services.AddHangfireServer();

var app = builder.Build();

// Настройка Recurring Job
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    var parserService = scope.ServiceProvider.GetRequiredService<NewsParserService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // Запускаем парсинг при старте
    await parserService.ParseAllTopicsAsync();

    // Периодический запуск каждые 10 минут
    recurringJobManager.AddOrUpdate(
        "parse-all-topics",
        () => parserService.ParseAllTopicsAsync(),
        "*/10 * * * *");
    
    logger.LogInformation("Hangfire scheduled: Parse all topics every 10 minutes");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.UseHangfireDashboard("/hangfire");

app.Run();