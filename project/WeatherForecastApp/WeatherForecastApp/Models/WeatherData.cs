using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherForecastApp.Models
{
    public class WeatherData
    {
        [JsonProperty("coord")]
        public Coord? Coord { get; set; }

        [JsonProperty("weather")]
        public List<WeatherDescription> Weather { get; set; }

        [JsonProperty("base")]
        public string? Base { get; set; }

        [JsonProperty("main")]
        public MainInfo Main { get; set; }

        [JsonProperty("visibility")]
        public int Visibility { get; set; }

        [JsonProperty("wind")]
        public WindInfo Wind { get; set; }

        [JsonProperty("clouds")]
        public Clouds? Clouds { get; set; }

        [JsonProperty("dt")]
        public long Dt { get; set; }

        [JsonProperty("sys")]
        public SysInfo? Sys { get; set; }

        [JsonProperty("timezone")]
        public int Timezone { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cod")]
        public int Cod { get; set; }

        // tostring method for debugging
        public override string ToString()
        {
            return $"\nWeatherData: {Name}, \n" +
                $"Temp: {Main.Temp}°C, \n" +
                $"Humidity: {Main.Humidity}%, \n" +
                $"Wind Speed: {Wind.Speed} m/s, \n" +
                $"Timestamp: {Dt}, \n" +
                $"Weather: {string.Join(", ", Weather.Select(w => w.Description))}";
        }
    }

    public class Coord
    {
        [JsonProperty("lon")]
        public double Lon { get; set; }

        [JsonProperty("lat")]
        public double Lat { get; set; }
    }

    public class WeatherDescription
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("main")]
        public string? Main { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("icon")]
        public string? Icon { get; set; }
    }

    public class MainInfo
    {
        [JsonProperty("temp")]
        public double Temp { get; set; }

        [JsonProperty("feels_like")]
        public double FeelsLike { get; set; }

        [JsonProperty("temp_min")]
        public double TempMin { get; set; }

        [JsonProperty("temp_max")]
        public double TempMax { get; set; }

        [JsonProperty("pressure")]
        public int Pressure { get; set; }

        [JsonProperty("humidity")]
        public int Humidity { get; set; }

        [JsonProperty("sea_level")]
        public int SeaLevel { get; set; }

        [JsonProperty("grnd_level")]
        public int GrndLevel { get; set; }
    }

    public class WindInfo
    {
        [JsonProperty("speed")]
        public double Speed { get; set; }

        [JsonProperty("deg")]
        public int Deg { get; set; }
    }

    public class Clouds
    {
        [JsonProperty("all")]
        public int All { get; set; }
    }

    public class SysInfo
    {
        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("country")]
        public string? Country { get; set; }

        [JsonProperty("sunrise")]
        public long Sunrise { get; set; }

        [JsonProperty("sunset")]
        public long Sunset { get; set; }
    }
}
