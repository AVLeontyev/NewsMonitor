using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NewsMonitor.Shared.Models;

namespace NewsMonitor.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<News> News { get; set; }
    public DbSet<UserTopic> UserTopics { get; set; }
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<NewsSource> NewsSources { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties()
                .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
            {
                property.SetValueConverter(new ValueConverter<DateTime, DateTime>(
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc),  
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc)  
                ));
            }
        }

        // Уникальный индекс для предотвращения дубликатов новостей
        modelBuilder.Entity<News>()
            .HasIndex(n => n.Hash)
            .IsUnique();

        // Составной ключ для UserTopic
        modelBuilder.Entity<UserTopic>()
            .HasKey(ut => new { ut.UserId, ut.TopicId });

        // Связи
        modelBuilder.Entity<UserTopic>()
            .HasOne(ut => ut.User)
            .WithMany(u => u.UserTopics)
            .HasForeignKey(ut => ut.UserId);

        modelBuilder.Entity<UserTopic>()
            .HasOne(ut => ut.Topic)
            .WithMany(t => t.UserTopics)
            .HasForeignKey(ut => ut.TopicId);

        modelBuilder.Entity<Alert>()
            .HasOne(a => a.News)
            .WithMany()
            .HasForeignKey(a => a.NewsId);

        modelBuilder.Entity<Alert>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId);
    }
}