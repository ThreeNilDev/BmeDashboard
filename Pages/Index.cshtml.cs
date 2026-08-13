using BmeDashboard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BmeDashboard.Pages;

public class IndexModel : PageModel
{
    private readonly IBme680Service _sensorService;
    private readonly WeatherService _weatherService;

    public double? TemperatureC { get; set; }
    public double? PressureHpa { get; set; }
    public double? HumidityPercent { get; set; }
    public string? ErrorMessage { get; set; }

    public double? OutdoorTemperatureC { get; set; }
    public double? OutdoorPressureHpa { get; set; }
    public double? OutdoorHumidity { get; set; }
    public string? WeatherDescription { get; set; }
    public DateTime? WeatherTimestamp { get; set; } = DateTime.MinValue;


    public IndexModel(IBme680Service sensorService, WeatherService weatherService)
    {
        _sensorService = sensorService;
        _weatherService = weatherService;
    }

    public async Task OnGetAsync()
    {
        try
        {
            var reading = await _sensorService.ReadSensorAsync();
            TemperatureC = reading.TemperatureC;
            PressureHpa = reading.PressureHpa;
            HumidityPercent = reading.HumidityPercent;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not read sensor: {ex.Message}";
        }


        try
        {
            var weather = await _weatherService.GetCurrentWeatherAsync();
            OutdoorTemperatureC = weather.TemperatureC;
            OutdoorHumidity = weather.Humidity;         // was weather.HumidityPercent
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
            timestamp = DateTime.Now.ToString("HH:mm:ss")
        });
    }
}