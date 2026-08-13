using Microsoft.EntityFrameworkCore;
using BmeDashboard.Models;

namespace BmeDashboard.Data;

public class SensorDbContext : DbContext
{
    public SensorDbContext(DbContextOptions<SensorDbContext> options) : base(options) { }

    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
    public DbSet<WeatherReading> WeatherReadings => Set<WeatherReading>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WeatherReading>()
            .HasIndex(w => w.TimestampUtc);
    }
}