namespace BmeDashboard.Services
{
    public class FakeBme680Service : IBme680Service
    {
        public Task<SensorReadingResult> ReadSensorAsync()
        {
            var fakeReading = new SensorReadingResult
            {
                TemperatureC = 21.5,
                PressureHpa = 1013.2,
                HumidityPercent = 45.0
            };
            return Task.FromResult(fakeReading);
        }
    }
}
