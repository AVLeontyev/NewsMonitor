using Hangfire;
using Hangfire.PostgreSql;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NewsMonitor.Shared.Data;
using NewsMonitor.Shared.Messages;
using NewsMonitor.Parser.Core.Services;
using NewsMonitor.API.Hubs;
using NewsMonitor.API.Services;
using Serilog;
using Serilog.Enrichers;
using Serilog.Sinks.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProcessId()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(builder.Configuration["Elasticsearch:Uri"]))
    {
        AutoRegisterTemplate = true,
        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
        IndexFormat = builder.Configuration["Elasticsearch:IndexFormat"] ?? "newsmonitor-api-{0:yyyy.MM.dd}",
        NumberOfShards = 1,
        NumberOfReplicas = 0,
        FailureCallback = (logEvent, exception) => 
            Console.WriteLine($"Elasticsearch error: {logEvent.MessageTemplate}"),
        EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                          EmitEventFailureHandling.WriteToFailureSink |
                          EmitEventFailureHandling.RaiseCallback
    })
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // любой origin
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddScoped<INotificationService, HttpNotificationService>(); // для парсера
builder.Services.AddScoped<RealTimeNotificationService>(); // для контроллера
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

app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// SignalR
app.MapHub<NewsHub>("/newshub");

app.UseHangfireDashboard("/hangfire");

app.Run();

Log.CloseAndFlush();