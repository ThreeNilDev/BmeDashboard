using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BmeDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddWeatherReadingTimestampIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WeatherReadings_TimestampUtc",
                table: "WeatherReadings",
                column: "TimestampUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WeatherReadings_TimestampUtc",
                table: "WeatherReadings");
        }
    }
}
