using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using WeatherForecastApp.Utils;

namespace WeatherForecastApp.Models
{
    public class WeatherInfo
    {
        [Key]
        public int Id { get; set; }

        public string Location { get; set; }
        public DateTime DateTime { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double WindSpeed { get; set; }
        public string WeatherCondition { get; set; }

        // Icon code from OpenWeatherMap (e.g., "01d")
        public string? IconCode { get; set; }

        // Full image URL, auto-generated
        public string? IconUrl => !string.IsNullOrWhiteSpace(IconCode)
            ? $"https://openweathermap.org/img/wn/{IconCode}@2x.png"
            : null;

        public string? GithubIconUrl => !string.IsNullOrWhiteSpace(IconCode)
            ? $"https://rodrigokamada.github.io/openweathermap/images/{IconCode}_t@2x.png"
            : null;

        public WeatherInfo(string location, DateTime dateTime, double temperature, double humidity, double windSpeed, string weatherCondition, string? iconCode = null)
        {
            Location = location;
            DateTime = dateTime;
            Temperature = temperature;
            Humidity = humidity;
            WindSpeed = windSpeed;
            WeatherCondition = weatherCondition;
            IconCode = iconCode;

        }

        public override string ToString()
        {
            return $"\nLocation: {Location}, \n" +
                $" Time: {DateTime:yyyy-MM-dd HH:mm}, \n" +
                $" Temperature: {Temperature}°C, \n" +
                $" Humidity: {Humidity}%, \n" +
                $" Wind Speed: {WindSpeed} m/s, \n" +
                $" Weather: {WeatherCondition}, \n" +
                $" Icon: {IconCode}, \n" +
                $" IconUrl: {IconUrl} \n" +
                $" GithubIconUrl: {GithubIconUrl}";
        }
    }

}
