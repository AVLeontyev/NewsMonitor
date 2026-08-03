using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NewsMonitor.Consumer.Consumers;
using NewsMonitor.Shared.Messages;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace NewsMonitor.Consumer;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        var elasticUri = builder.Configuration["Elasticsearch:Uri"] ?? "http://elasticsearch:9200";

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
                IndexFormat = "newsmonitor-consumer-{0:yyyy.MM.dd}",
                NumberOfShards = 1,
                NumberOfReplicas = 0
            })
            .CreateLogger();

        builder.Services.AddSerilog();

        var logger = Log.ForContext<Program>();

        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
        var rabbitPort = builder.Configuration["RabbitMQ:Port"] ?? "5672";
        var rabbitUser = builder.Configuration["RabbitMQ:Username"] ?? "newsuser";
        var rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? "newspassword";
        var rabbitVHost = builder.Configuration["RabbitMQ:VirtualHost"] ?? "newsmonitor";

        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumer<NewsCreatedConsumer>();
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri($"rabbitmq://{rabbitHost}:{rabbitPort}/{rabbitVHost}"), h =>
                {
                    h.Username(rabbitUser);
                    h.Password(rabbitPass);
                });
                
                cfg.ReceiveEndpoint("news_queue", e =>
                {
                    e.ConfigureConsumer<NewsCreatedConsumer>(context);
                });
            });
        });

        builder.Services.AddMassTransitHostedService();

        var host = builder.Build();

        await host.RunAsync();

        Log.CloseAndFlush();
    }
}