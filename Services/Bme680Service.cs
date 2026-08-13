using Iot.Device.Bmxx80;
using Iot.Device.Common;
using System.Device.I2c;
using UnitsNet;

namespace BmeDashboard.Services;

public class SensorReadingResult
{
    public double? TemperatureC { get; init; }
    public double? PressureHpa { get; init; }
    public double? HumidityPercent { get; init; }
    public double? DewpointC { get; init; }
    public double? HeatIndexC { get; init; }
    public double? AltitudeMeters { get; init; }
}

public interface IBme680Service
{
    Task<SensorReadingResult> ReadSensorAsync();
}

public class Bme680Service : IBme680Service
{
    private readonly IWeatherService _weatherService;

    public Bme680Service(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    public async Task<SensorReadingResult> ReadSensorAsync()
    {
        var i2cSettings = new I2cConnectionSettings(1, Bme680.DefaultI2cAddress);
        using var i2cDevice = I2cDevice.Create(i2cSettings);
        using var bme680 = new Bme680(i2cDevice);

        var readResult = await bme680.ReadAsync();

        double? altitudeMeters = null;
        if (readResult.Pressure.HasValue && readResult.Temperature.HasValue)
        {
            Pressure seaLevelPressure;
            try
            {
                var weather = await _weatherService.GetCurrentWeatherAsync();
                seaLevelPressure = weather.PressureHpa.HasValue
                    ? Pressure.FromHectopascals(weather.PressureHpa.Value)
                    : Pressure.FromHectopascals(1013.25); // fallback if API returned no pressure
            }
            catch
            {
                seaLevelPressure = Pressure.FromHectopascals(1013.25); // fallback if API call failed
            }

            altitudeMeters = WeatherHelper
                .CalculateAltitude(readResult.Pressure.Value, seaLevelPressure, readResult.Temperature.Value)
                .Meters;
        }

        double? dewpointC = null;
        if (readResult.Temperature.HasValue && readResult.Humidity.HasValue)
        {
            dewpointC = WeatherHelper
                .CalculateDewPoint(readResult.Temperature.Value, readResult.Humidity.Value)
                .DegreesCelsius;
        }

        double? heatIndexC = null;
        if (readResult.Temperature.HasValue && readResult.Humidity.HasValue)
        {
            heatIndexC = WeatherHelper
                .CalculateHeatIndex(readResult.Temperature.Value, readResult.Humidity.Value)
                .DegreesCelsius;
        }

        return new SensorReadingResult
        {
            TemperatureC = readResult.Temperature?.DegreesCelsius,
            PressureHpa = readResult.Pressure?.Hectopascals,
            HumidityPercent = readResult.Humidity?.Percent,
            DewpointC = dewpointC,
            HeatIndexC = heatIndexC,
            AltitudeMeters = altitudeMeters
        };
    }
}