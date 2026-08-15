using BmeDashboard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;

namespace BmeDashboard.Pages;

public class IndexModel : PageModel
{
    private readonly IBme680Service _sensorService;
    private readonly IWeatherService _weatherService;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;

    public double? TemperatureC { get; set; }
    public double? PressureHpa { get; set; }
    public double? HumidityPercent { get; set; }
    public string? ErrorMessage { get; set; }
    public double? DewpointC { get; set; }
    public double? HeatIndexC { get; set; }
    public double? AltitudeMeters { get; set; }


    public double? OutdoorTemperatureC { get; set; }
    public double? OutdoorPressureHpa { get; set; }
    public double? OutdoorHumidity { get; set; }
    public string? WeatherDescription { get; set; }
    public DateTime? WeatherTimestamp { get; set; } = DateTime.MinValue;


    public IndexModel(IBme680Service sensorService, IWeatherService weatherService, IHttpClientFactory httpClientFactory, IMemoryCache cache)
    {
        _sensorService = sensorService;
        _weatherService = weatherService;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
    }

    public async Task OnGetAsync()
    {
        try
        {
            var reading = await _sensorService.ReadSensorAsync();
            TemperatureC = reading.TemperatureC;
            PressureHpa = reading.PressureHpa;
            HumidityPercent = reading.HumidityPercent;
            DewpointC = reading.DewpointC;
            HeatIndexC = reading.HeatIndexC;
            AltitudeMeters = reading.AltitudeMeters;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not read sensor: {ex.Message}";
        }


        try
        {
            var weather = await _weatherService.GetCurrentWeatherAsync();
            OutdoorTemperatureC = weather.TemperatureC;
            OutdoorHumidity = weather.HumidityPercent;
            OutdoorPressureHpa = weather.PressureHpa;
            WeatherDescription = weather.Description;
            WeatherTimestamp = weather.Timestamp;
        }
        catch (Exception)
        {
            WeatherDescription = "Weather unavailable";
        }
    }

    public async Task<JsonResult> OnGetLatestReadingAsync()
    {
        var reading = await _sensorService.ReadSensorAsync();
        return new JsonResult(new
        {
            temperature = reading.TemperatureC,
            pressure = reading.PressureHpa,
            humidity = reading.HumidityPercent,
            timestamp = DateTime.Now.ToString("HH:mm:ss"),
            dewpoint = reading.DewpointC,
            heatIndex = reading.HeatIndexC,
            altitude = reading.AltitudeMeters

        });
    }

    public async Task<IActionResult> OnGetWikiSummaryAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return BadRequest();

        string cacheKey = $"wiki:{term.ToLowerInvariant()}";

        if (_cache.TryGetValue(cacheKey, out string cachedJson))
        {
            return Content(cachedJson, "application/json");
        }

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("BmeDashboard/1.0 (https://github.com/ThreeNilDev/BmeDashboard)");

        var url = $"https://en.wikipedia.org/api/rest_v1/page/summary/{Uri.EscapeDataString(term)}";
        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        _cache.Set(cacheKey, json, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        });

        return Content(json, "application/json");
    }
}