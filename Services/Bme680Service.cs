using Iot.Device.Bmxx80;
using System.Device.I2c;

namespace BmeDashboard.Services;

public class Bme680Service
{
    public async Task<(double? TemperatureC, double? PressureHpa, double? HumidityPercent)> ReadSensorAsync()
    {
        var i2cSettings = new I2cConnectionSettings(1, Bme680.DefaultI2cAddress);
        using var i2cDevice = I2cDevice.Create(i2cSettings);
        using var bme680 = new Bme680(i2cDevice);

        var readResult = await bme680.ReadAsync();

        return (
            readResult.Temperature?.DegreesCelsius,
            readResult.Pressure?.Hectopascals,
            readResult.Humidity?.Percent
        );
    }
}