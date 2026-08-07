using BmeDashboard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BmeDashboard.Pages;

public class IndexModel : PageModel
{
    private readonly Bme680Service _sensorService;

    public double? TemperatureC { get; set; }
    public double? PressureHpa { get; set; }
    public double? HumidityPercent { get; set; }
    public string? ErrorMessage { get; set; }

    public IndexModel(Bme680Service sensorService)
    {
        _sensorService = sensorService;
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