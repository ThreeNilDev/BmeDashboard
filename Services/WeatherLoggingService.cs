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

            var intervalMinutes = config.GetValue<int?>("WeatherLogging:IntervalMinutes")
    ?? throw new InvalidOperationException("WeatherLogging:IntervalMinutes is not configured.");

            if (intervalMinutes <= 0)
                throw new InvalidOperationException("WeatherLogging:IntervalMinutes must be greater than zero.");

            _interval = TimeSpan.FromMinutes(intervalMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<SensorDbContext>();
                    var weatherService = scope.ServiceProvider.GetRequiredService<IWeatherService>();

                    var weatherData = await weatherService.GetCurrentWeatherAsync();

                    db.WeatherReadings.Add(new WeatherReading
                    {
                        TimestampUtc = weatherData.Timestamp,
                        TemperatureC = weatherData.TemperatureC,
                        PressureHpa = weatherData.PressureHpa,
                        HumidityPercent = weatherData.HumidityPercent,
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