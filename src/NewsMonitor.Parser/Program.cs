using Hangfire;
using Hangfire.PostgreSql;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NewsMonitor.Parser.Core.Services;
using NewsMonitor.Shared.Data;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace NewsMonitor.Parser;

public class Program
{
    public static async Task Main(string[] args)
    {
         Log.Logger = new LoggerConfiguration()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProcessId()
            .WriteTo.Console()
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
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
        logger.Information("Application starting...");

        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        var connectionString = "Server=127.0.0.1;Port=5432;Database=newsmonitor;User Id=postgres;Password=postgres;SSL Mode=Disable;";
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        builder.Services.AddScoped<INotificationService, NewsMonitor.Parser.Core.Services.NotificationService>();
        builder.Services.AddScoped<NewsParserService>();
        builder.Services.AddHttpClient();

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

        builder.Services.AddHangfire(config =>
            config.UsePostgreSqlStorage(connectionString));
        builder.Services.AddHangfireServer();

        builder.Services.AddScoped<NewsParserService>();

        var host = builder.Build();

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var parserService = services.GetRequiredService<NewsParserService>();
            var logger = services.GetRequiredService<ILogger<Program>>();
            var recurringJobManager = services.GetRequiredService<IRecurringJobManager>();

            await parserService.ParseAllTopicsAsync();

            recurringJobManager.AddOrUpdate(
                "parse-all-topics",
                () => parserService.ParseAllTopicsAsync(),
                "*/10 * * * *");
            
            logger.LogInformation("Hangfire scheduled: Parse all topics every 10 minutes");
        }

        await host.RunAsync();

        Log.CloseAndFlush();
    }
}