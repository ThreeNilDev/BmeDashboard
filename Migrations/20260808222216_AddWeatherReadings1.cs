using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BmeDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddWeatherReadings1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeatherReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TimestampUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TemperatureC = table.Column<double>(type: "REAL", nullable: true),
                    PressureHpa = table.Column<double>(type: "REAL", nullable: true),
                    HumidityPercent = table.Column<double>(type: "REAL", nullable: true),
                    WeatherDescription = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherReadings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeatherReadings");
        }
    }
}
