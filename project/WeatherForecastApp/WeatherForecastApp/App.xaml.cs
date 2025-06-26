using System.Configuration;
using System.Data;
using System.Windows;
using WeatherForecastApp.Data;

namespace WeatherForecastApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Initialize any required services or configurations here
            // For example, you might want to set up a database connection or load settings
            

        }
        protected override void OnExit(ExitEventArgs e)
        {
            // Clean up resources if necessary
            base.OnExit(e);
        }
    }

}
