using BmeDashboard.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BmeDashboard.Pages;

public class HistoryModel : PageModel
{
    private readonly SensorDbContext _db;

    public HistoryModel(SensorDbContext db)
    {
        _db = db;
    }

    public void OnGet()
    {
        // Page just loads; data is fetched via JS calling the handlers below
    }

    public async Task<JsonResult> OnGetChartDataAsync(int days = 1, bool hourly = false)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);

        var readings = await _db.SensorReadings
            .Where(r => r.TimestampUtc >= cutoff)
            .OrderBy(r => r.TimestampUtc)
            .ToListAsync();

        if (!hourly)
        {
            return new JsonResult(readings.Select(r => new
            {
                timestamp = r.TimestampUtc,
                temperature = r.TemperatureC,
                pressure = r.PressureHpa,
                humidity = r.HumidityPercent
            }));
        }

        var hourlyAverages = readings
            .GroupBy(r => new DateTime(r.TimestampUtc.Year, r.TimestampUtc.Month, r.TimestampUtc.Day, r.TimestampUtc.Hour, 0, 0))
            .Select(g => new
            {
                timestamp = g.Key,
                temperature = g.Average(r => r.TemperatureC),
                pressure = g.Average(r => r.PressureHpa),
                humidity = g.Average(r => r.HumidityPercent)
            })
            .OrderBy(g => g.timestamp)
            .ToList();

        return new JsonResult(hourlyAverages);
    }

    public async Task<JsonResult> OnGetWeatherChartDataAsync(int days = 1, bool hourly = false)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);

        var readings = await _db.WeatherReadings
            .Where(r => r.TimestampUtc >= cutoff)
            .OrderBy(r => r.TimestampUtc)
            .ToListAsync();

        if (!hourly)
        {
            return new JsonResult(readings.Select(r => new
            {
                timestamp = r.TimestampUtc,
                temperature = r.TemperatureC,
                pressure = r.PressureHpa,
                humidity = r.HumidityPercent
            }));
        }

        var hourlyAverages = readings
            .GroupBy(r => new DateTime(r.TimestampUtc.Year, r.TimestampUtc.Month, r.TimestampUtc.Day, r.TimestampUtc.Hour, 0, 0))
            .Select(g => new
            {
                timestamp = g.Key,
                temperature = g.Average(r => r.TemperatureC),
                pressure = g.Average(r => r.PressureHpa),
                humidity = g.Average(r => r.HumidityPercent)
            })
            .OrderBy(g => g.timestamp)
            .ToList();

        return new JsonResult(hourlyAverages);
    }
}