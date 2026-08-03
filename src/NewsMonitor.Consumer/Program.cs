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
                IndexFormat = "newsmonitor-consumer-{0:yyyy.MM.dd}",
                NumberOfShards = 1,
                NumberOfReplicas = 0
            })
            .CreateLogger();

            
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddSerilog();

        var logger = Log.ForContext<Program>();

        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumer<NewsCreatedConsumer>();
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri("rabbitmq://localhost/newsmonitor"), h =>
                {
                    h.Username("newsuser");
                    h.Password("newspassword");
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