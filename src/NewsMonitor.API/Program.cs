using Hangfire;
using Hangfire.PostgreSql;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NewsMonitor.API.Data;
using NewsMonitor.Shared.Messages;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрация DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"=== DIAGNOSTIC ===");
Console.WriteLine($"Connection String from config: {connectionString}");
Console.WriteLine($"ASPNETCORE_ENVIRONMENT: {builder.Environment.EnvironmentName}");
Console.WriteLine($"==================");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Настройка MassTransit 7.x
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
    config.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Добавление Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

app.Run();