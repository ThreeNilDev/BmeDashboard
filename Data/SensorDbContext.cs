using Microsoft.EntityFrameworkCore;
using BmeDashboard.Models;

namespace BmeDashboard.Data;

public class SensorDbContext : DbContext
{
    public SensorDbContext(DbContextOptions<SensorDbContext> options) : base(options) { }

    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
}