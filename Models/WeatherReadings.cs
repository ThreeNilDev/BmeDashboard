namespace BmeDashboard.Models
{
    public class WeatherReading
    {
        public int Id { get; set; }
        public DateTime TimestampUtc { get; set; }
        public double? TemperatureC { get; set; }
        public double? PressureHpa { get; set; }
        public double? HumidityPercent { get; set; }

        public string WeatherDescription { get; set; } = string.Empty;
    }
}
