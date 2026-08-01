using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NewsMonitor.Consumer.Consumers;
using NewsMonitor.Shared.Messages;

namespace NewsMonitor.Consumer;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

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
    }
}