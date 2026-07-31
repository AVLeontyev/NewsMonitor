using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NewsMonitor.API.Data;

namespace NewsMonitor.API.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql("Server=127.0.0.1;Port=5432;Database=newsmonitor;User Id=postgres;Password=postgres;SSL Mode=Disable;");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}