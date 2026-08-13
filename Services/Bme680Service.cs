using Iot.Device.Bmxx80;
using System.Device.I2c;

namespace BmeDashboard.Services;

public class SensorReadingResult
{
    public double? TemperatureC { get; init; }
    public double? PressureHpa { get; init; }
    public double? HumidityPercent { get; init; }
}

public interface IBme680Service
{
    Task<SensorReadingResult> ReadSensorAsync();
}

public class Bme680Service : IBme680Service
{
    public async Task<SensorReadingResult> ReadSensorAsync()
    {
        var i2cSettings = new I2cConnectionSettings(1, Bme680.DefaultI2cAddress);
        using var i2cDevice = I2cDevice.Create(i2cSettings);
        using var bme680 = new Bme680(i2cDevice);

        var readResult = await bme680.ReadAsync();

        return new SensorReadingResult
        {
            TemperatureC = readResult.Temperature?.DegreesCelsius,
            PressureHpa = readResult.Pressure?.Hectopascals,
            HumidityPercent = readResult.Humidity?.Percent
        };
    }
}