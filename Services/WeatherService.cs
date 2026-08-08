using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace BmeDashboard.Services;

public record WeatherResult(
    double? TemperatureC,
    double? Humidity,
    double? PressureHpa,
    string? Description,
    DateTime Timestamp
);

public class WeatherService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration;
    private const string CacheKey = "CurrentWeather";

    private const double Latitude = 50.88;
    private const double Longitude = -1.03;
    

    public WeatherService(HttpClient httpClient, IMemoryCache cache, IConfiguration config)
    {
        _httpClient = httpClient;
        _cache = cache;
        _cacheDuration = TimeSpan.FromMinutes(config.GetValue<int>("Weather:CacheMinutes"));
    }

    public async Task<WeatherResult> GetCurrentWeatherAsync()
    {
        if (_cache.TryGetValue(CacheKey, out WeatherResult cachedWeather))
        {
            return cachedWeather;
        }

        var url = $"https://api.open-meteo.com/v1/forecast?latitude={Latitude.ToString()}&longitude={Longitude.ToString()}&current=temperature_2m,relative_humidity_2m,surface_pressure,weather_code";

        var response = await _httpClient.GetStringAsync(url);
        using var doc = JsonDocument.Parse(response);
        var current = doc.RootElement.GetProperty("current");

        var temp = current.GetProperty("temperature_2m").GetDouble();
        var humidity = current.GetProperty("relative_humidity_2m").GetDouble();
        var pressure = current.GetProperty("surface_pressure").GetDouble();
        var weatherCode = current.GetProperty("weather_code").GetInt32();
        var description = WeatherCodeToDescription(weatherCode);
        var currentDateTime = DateTime.UtcNow;

        var result = new WeatherResult(
            TemperatureC: (double?)temp,
            Humidity: (double?)humidity,
            PressureHpa: (double?)pressure,
            Description: description,
            Timestamp: currentDateTime
        );

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