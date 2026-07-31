using Microsoft.EntityFrameworkCore;
using NewsMonitor.API.Data;  // <-- Изменено: теперь Data в API проекте
using NewsMonitor.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Регистрация DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"=== DIAGNOSTIC ===");
Console.WriteLine($"Connection String from config: {connectionString}");
Console.WriteLine($"ASPNETCORE_ENVIRONMENT: {builder.Environment.EnvironmentName}");
Console.WriteLine($"==================");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрация DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

app.Run();