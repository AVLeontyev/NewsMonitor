using Hangfire;
using Hangfire.PostgreSql;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewsMonitor.Parser.Core.Services;
using NewsMonitor.Shared.Data;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace NewsMonitor.Parser;

public class Program
{
    public static async Task Main(string[] args)
    {
        var elasticUri = Environment.GetEnvironmentVariable("Elasticsearch__Uri") 
            ?? "http://elasticsearch:9200";

        // Настройка Serilog
        Log.Logger = new LoggerConfiguration()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProcessId()
            .WriteTo.Console()
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUri))  
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                IndexFormat = "newsmonitor-parser-{0:yyyy.MM.dd}",
                NumberOfShards = 1,
                NumberOfReplicas = 0
            })
            .CreateLogger();

        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddSerilog();

        var logger = Log.ForContext<Program>();

        // Настройка DbContext
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") 
            ?? "Host=postgres;Port=5432;Database=newsmonitor;Username=postgres;Password=postgres;SSL Mode=Disable;";

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Настройка HttpClient
        builder.Services.AddHttpClient();

        // Настройка MassTransit
        var rabbitHost = Environment.GetEnvironmentVariable("RabbitMQ__Host") ?? "rabbitmq";
        var rabbitPort = Environment.GetEnvironmentVariable("RabbitMQ__Port") ?? "5672";
        var rabbitUser = Environment.GetEnvironmentVariable("RabbitMQ__Username") ?? "newsuser";
        var rabbitPass = Environment.GetEnvironmentVariable("RabbitMQ__Password") ?? "newspassword";
        var rabbitVHost = Environment.GetEnvironmentVariable("RabbitMQ__VirtualHost") ?? "newsmonitor";

        builder.Services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
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

        // Регистрация сервисов
        builder.Services.AddScoped<INotificationService, HttpNotificationService>();
        builder.Services.AddScoped<NewsParserService>();
        builder.Services.AddHttpClient();

        var host = builder.Build();

        // Настройка Hangfire задач
        using (var scope = host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            //await dbContext.Database.EnsureCreatedAsync();

            var services = scope.ServiceProvider;
            var parserService = services.GetRequiredService<NewsParserService>();
            var recurringJobManager = services.GetRequiredService<IRecurringJobManager>();

            logger.Information("Parser service starting...");
            
            // Запускаем парсинг сразу при старте
            await parserService.ParseAllTopicsAsync();

            // Настраиваем периодический запуск (каждые 10 минут)
            recurringJobManager.AddOrUpdate(
                "parse-all-topics",
                () => parserService.ParseAllTopicsAsync(),
                "*/10 * * * *");
            
            logger.Information("Hangfire scheduled: Parse all topics every 10 minutes");
        }

        await host.RunAsync();
        
        Log.CloseAndFlush();
    }
}