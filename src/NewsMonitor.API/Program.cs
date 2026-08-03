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
using Hangfire.Dashboard;

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
        var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
        var rabbitPort = builder.Configuration["RabbitMQ:Port"] ?? "5672";
        var rabbitUser = builder.Configuration["RabbitMQ:Username"] ?? "newsuser";
        var rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? "newspassword";
        var rabbitVHost = builder.Configuration["RabbitMQ:VirtualHost"] ?? "newsmonitor";

        cfg.Host(new Uri($"rabbitmq://{rabbitHost}:{rabbitPort}/{rabbitVHost}"), h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
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
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    //await dbContext.Database.EnsureCreatedAsync();

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

// авторизация отключена
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AllowAllAuthorizationFilter() } 
});

app.Run();

Log.CloseAndFlush();
public class AllowAllAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}