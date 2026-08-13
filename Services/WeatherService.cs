using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace BmeDashboard.Services;

public class WeatherReadingResult
{
    public double? TemperatureC { get; init; }
    public double? HumidityPercent { get; init; }
    public double? PressureHpa { get; init; }
    public string? Description { get; init; }
    public DateTime Timestamp { get; init; }
}

public interface IWeatherService
{
    Task<WeatherReadingResult> GetCurrentWeatherAsync();
}

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration;
    private const string CacheKey = "CurrentWeather";

    private readonly double _latitude;
    private readonly double _longitude;

    public WeatherService(HttpClient httpClient, IMemoryCache cache, IConfiguration config)
    {
        _httpClient = httpClient;
        _cache = cache;
        _cacheDuration = TimeSpan.FromMinutes(config.GetValue<int>("Weather:CacheMinutes"));
        _latitude = config.GetValue<double>("Weather:Latitude");
        _longitude = config.GetValue<double>("Weather:Longitude");
    }

    public async Task<WeatherReadingResult> GetCurrentWeatherAsync()
    {
        if (_cache.TryGetValue(CacheKey, out WeatherReadingResult? cachedWeather) && cachedWeather is not null)
        {
            return cachedWeather;
        }

        var url = $"https://api.open-meteo.com/v1/forecast?latitude={_latitude}&longitude={_longitude}&current=temperature_2m,relative_humidity_2m,pressure_msl,weather_code";

        var response = await _httpClient.GetStringAsync(url);
        using var doc = JsonDocument.Parse(response);
        var current = doc.RootElement.GetProperty("current");

        double? temperatureC = current.TryGetProperty("temperature_2m", out var tempProp)
            ? tempProp.GetDouble()
            : null;

        double? humidityPercent = current.TryGetProperty("relative_humidity_2m", out var humidityProp)
            ? humidityProp.GetDouble()
            : null;

        double? pressureHpa = current.TryGetProperty("pressure_msl", out var pressureProp)
            ? pressureProp.GetDouble()
            : null;

        string? description = current.TryGetProperty("weather_code", out var codeProp)
            ? WeatherCodeToDescription(codeProp.GetInt32())
            : null;

        var result = new WeatherReadingResult
        {
            TemperatureC = temperatureC,
            HumidityPercent = humidityPercent,
            PressureHpa = pressureHpa,
            Description = description,
            Timestamp = DateTime.UtcNow
        };

        _cache.Set(CacheKey, result, _cacheDuration);

        return result;
    }

    private static string WeatherCodeToDescription(int code) => code switch
    {
        0 => "Clear sky",
        1 or 2 or 3 => "Partly cloudy",
        45 or 48 => "Fog",
        51 or 53 or 55 => "Drizzle",
        61 or 63 or 65 => "Rain",
        71 or 73 or 75 => "Snow",
        80 or 81 or 82 => "Rain showers",
        95 => "Thunderstorm",
        _ => "Unknown"
    };
}