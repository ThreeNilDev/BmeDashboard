using BmeDashboard.Data;
using BmeDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace BmeDashboard.Services;


public class SensorLoggingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SensorLoggingService> _logger;
    private readonly TimeSpan _interval;

    public SensorLoggingService(IServiceProvider serviceProvider, ILogger<SensorLoggingService> logger, IConfiguration config)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _interval = TimeSpan.FromMinutes(config.GetValue<int>("SensorLogging:IntervalMinutes")); // adjust as needed in app settings
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var sensorService = scope.ServiceProvider.GetRequiredService<Bme680Service>();
                var db = scope.ServiceProvider.GetRequiredService<SensorDbContext>();

                var result = await sensorService.ReadSensorAsync();

                db.SensorReadings.Add(new SensorReading
                {
                    TimestampUtc = DateTime.UtcNow,
                    TemperatureC = result.TemperatureC,
                    PressureHpa = result.PressureHpa,
                    HumidityPercent = result.HumidityPercent
                });

                await db.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Sensor reading saved at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log sensor reading");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}