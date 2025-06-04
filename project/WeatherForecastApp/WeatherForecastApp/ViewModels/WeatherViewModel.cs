using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WeatherForecastApp.Data.Repository;
using WeatherForecastApp.Models;
using WeatherForecastApp.Utils;

namespace WeatherForecastApp.ViewModels
{
    public class WeatherViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        // Add properties and methods for the ViewModel here
        // For example, you might have a property for the weather data and methods to load it
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        // weather repository interface
        private readonly IWeather<WeatherInfo> _weatherRepository;


        // WeatherInfo property
        private WeatherInfo _currentWeather;
        public WeatherInfo CurrentWeather
        {
            get => _currentWeather;
            set
            {
                Util.Log($"Setting CurrentWeather to {value}");
                Util.Log($"CurrentWeather: {_currentWeather}");
                if (_currentWeather != value)
                {
                    _currentWeather = value;
                    OnPropertyChanged(nameof(CurrentWeather));
                }
            }
        }

        private string _city;
        public string City
        {
            get => _city;
            set
            {
                _city = value;
                OnPropertyChanged(nameof(City));
            }
        }

        public ObservableCollection<WeatherInfo> FutureWeather { get; set; }

        // ICommand: FectherWeather
        public ICommand FetchWeatherCommand { get; set; }

        public WeatherViewModel()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Util.LogWithThread("WeatherViewModel constructor called");
            _weatherRepository = new WeatherRepository();
            FutureWeather = new ObservableCollection<WeatherInfo>();
            FetchWeatherCommand = new RelayCommand(FetchWeather);

            // Initialize non-nullable fields with default values
            _currentWeather = new WeatherInfo("Default Location", DateTime.Now, 20.0, 50.0, 5.0, "clear sky", "01d");
            _city = "Default City";

            // Initialize with some default weather data
            CurrentWeather = _currentWeather;

            // Add current weather data to Weather repository
            _weatherRepository.AddAsync(CurrentWeather).Wait();
        }

        private async void FetchWeather()
        {
            try
            {
                // Get current weather data from OpenWeatherMap API
                var currentData = await GetResponseData<WeatherData>("weather");

                if (currentData == null)
                {
                    MessageBox.Show("Unable to retrieve current weather data. Please check the city name or network connection.");
                    return;
                }

                Util.Log($"current Data: {currentData}");
                CurrentWeather = new WeatherInfo(
                    currentData.Name,
                    DateTimeOffset.FromUnixTimeSeconds(currentData.Dt).LocalDateTime,
                    currentData.Main.Temp,
                    currentData.Main.Humidity,
                    currentData.Wind.Speed,
                    currentData.Weather.FirstOrDefault()?.Description ?? "N/A",
                    currentData.Weather.FirstOrDefault()?.Icon
                );  
                // add current weather data to Weather repository
                await _weatherRepository.AddAsync(CurrentWeather);

                // Get future weather data from OpenWeatherMap API
                var forecastData = await GetResponseData<ForecastData>("forecast");
                
                if (forecastData == null || forecastData.List == null || !forecastData.List.Any())
                {
                    MessageBox.Show("Unable to retrieve future weather data. Please check the city name or network connection.");
                    return;
                }

                FutureWeather.Clear();
                foreach (var item in forecastData.List)
                {
                    FutureWeather.Add(new WeatherInfo(
                        forecastData.City?.Name ?? "N/A", 
                        DateTime.Parse(item.DtTxt),
                        item.Main.Temp,
                        item.Main.Humidity,
                        item.Wind.Speed,
                        item.Weather.FirstOrDefault()?.Description ?? "N/A",
                        item.Weather.FirstOrDefault()?.Icon
                    ));
                    // add future weather data to Weather repository
                    await _weatherRepository.AddAsync(FutureWeather.Last());
                }
            }
            catch (Exception ex)
            {
                // Handle any errors that occur during the API request
                Util.Log($"Error fetching weather data: {ex.Message}");
            }
        }

        private async Task<T?> GetResponseData<T>(string endpoint)
        {
            string apiKey = "5268dc817a4cd6e4c02c62fe06b8a68c";
            string city = City;
            string apiUrl = $"http://api.openweathermap.org/data/2.5/{endpoint}?q={city}&appid={apiKey}&units=metric";

            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();

                    var weatherData = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<T>(weatherData);

                    if (result == null)
                    {
                        throw new InvalidOperationException($"Failed to deserialize response data for endpoint: {endpoint}");
                    }

                    return result;
                }
                catch (HttpRequestException ex)
                {
                    string detail = $"Error: {ex.Message}\n";
                    if (ex.InnerException != null)
                        detail += $"Inner Exception: {ex.InnerException.Message}";
                    MessageBox.Show(detail);
                    Util.Log($"Network error: {detail}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error：" + ex.Message);
                }
            }

            return default; // Return default value if an error occurs or deserialization fails
        }


    }

}
