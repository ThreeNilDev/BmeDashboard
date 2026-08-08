using BmeDashboard.Data;
using BmeDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace BmeDashboard.Services
{
    public class WeatherLoggingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WeatherLoggingService> _logger;
        private readonly TimeSpan _interval;

        public WeatherLoggingService(IServiceProvider serviceProvider, ILogger<WeatherLoggingService> logger, IConfiguration config)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _interval = TimeSpan.FromMinutes(config.GetValue<int>("WeatherLogging:IntervalMinutes"));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<SensorDbContext>();
                    var weatherService = scope.ServiceProvider.GetRequiredService<WeatherService>();

                    var weatherData = await weatherService.GetCurrentWeatherAsync();

                    db.WeatherReadings.Add(new WeatherReading
                    {
                        TimestampUtc = weatherData.Timestamp,
                        TemperatureC = weatherData.TemperatureC,
                        PressureHpa = weatherData.PressureHpa,
                        HumidityPercent = weatherData.Humidity,
                        WeatherDescription = weatherData.Description
                    });

                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Weather data saved at {Time}", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to log weather data");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}