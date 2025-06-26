using Microsoft.EntityFrameworkCore.Internal;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WeatherForecastApp.Data;
using WeatherForecastApp.Utils;
using WeatherForecastApp.ViewModels;

namespace WeatherForecastApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // AppDbContext initialization
        private readonly AppDbContext _dbContext = DbContextFactory.Instance;

        // Test flag for network access
        private bool _testNetworkAccess = false;

        public MainWindow()
        {
            InitializeComponent();
            // Set the DataContext for data binding
            this.DataContext = new WeatherViewModel();
            // Test network access by loading a weather icon
            if (_testNetworkAccess)
            {
                LoadWeatherIconAsync("https://openweathermap.org/img/wn/10d@2x.png");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // create the database if it doesn't exist
            _dbContext.Database.EnsureCreated();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            // Dispose of the DbContext when the window is closing
            _dbContext.Dispose();
            base.OnClosing(e);
        }

        // Test use: Click event handler to test network access
        private async void TestNetwork_Click(object sender, RoutedEventArgs e)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

            using var client = new HttpClient();
            try
            {
                var response = await client.GetAsync("https://openweathermap.org/img/wn/10d@2x.png");
                response.EnsureSuccessStatusCode(); // 如果失敗會擲出例外

                string result = await response.Content.ReadAsStringAsync();
                MessageBox.Show("成功存取！資料:\n" + result);
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
                MessageBox.Show("錯誤：" + ex.Message);
            }
        }

        // Test use: Asynchronously load the weather icon from the given URL
        private async void LoadWeatherIconAsync(string url)
        {
           
            try
            {
                var httpClientHandler = new HttpClientHandler
                {
                    SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,
                    UseDefaultCredentials = true
                };

                using (var client = new HttpClient(httpClientHandler))
                {
     

                    var data = await client.GetByteArrayAsync(url);
                    var bitmap = new BitmapImage();
                    using (var stream = new MemoryStream(data))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }
                    //WeatherIcon.Source = bitmap;
                }
            }

            catch (HttpRequestException ex)
            {
                string detail = $"Error: {ex.Message}\n";
                if (ex.InnerException != null)
                {
                    detail += $"Inner Exception: {ex.InnerException.Message}";
                    if (ex.InnerException.InnerException != null)
                    {
                        detail += $"\nInner Inner Exception: {ex.InnerException.InnerException.Message}";
                    }
                }
                MessageBox.Show(detail);
                Util.Log($"Network error: {detail}");
            }
             
            catch (Exception ex)
            {
                MessageBox.Show("載入圖示失敗: " + ex.Message);
            }
        }

    }
}