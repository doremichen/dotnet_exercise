
using Newtonsoft.Json;

namespace WeatherForecastApp.Models
{
    public class ForecastData
    {
        [JsonProperty("list")]
        public List<ForecastItem> List { get; set; }

        [JsonProperty("city")]
        public CityInfo? City { get; set; }
    }

    public class ForecastItem
    {
        [JsonProperty("dt_txt")]
        public string DtTxt { get; set; }

        [JsonProperty("main")]
        public MainInfo Main { get; set; }

        [JsonProperty("wind")]
        public WindInfo Wind { get; set; }

        [JsonProperty("weather")]
        public List<WeatherDescription> Weather { get; set; }
    }

    public class CityInfo
    {
        [JsonProperty("name")]
        public string? Name { get; set; }
    }
}