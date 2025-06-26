using SQLitePCL;
using System.Configuration;
using System.Data;
using System.Windows;

namespace StudentGradeManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {

            Batteries_V2.Init(); // Initialize the Batteries library if used

            base.OnStartup(e);
            // Initialize the application, e.g., set up services, load data, etc.
            // This is a good place to set up dependency injection or other initializations.
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            // Clean up resources, save settings, etc. before the application exits.
        }
    }

}
