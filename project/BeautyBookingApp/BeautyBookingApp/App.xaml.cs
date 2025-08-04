using BeautyBookingApp.Logs;
using BeautyBookingApp.Services;
using BeautyBookingApp.Utils;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace BeautyBookingApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        /**
         * notify upcoming bookings on startup
         */
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // initialize audit logger
            if (ServiceLocator.AuditLogger == null)
            {
                string logPath = AppDataHelper.GetAppDataFilePath("audit.log");
                ServiceLocator.AuditLogger = new FileAuditLogger(logPath);
                // log application startup
                ServiceLocator.AuditLogger.Log("System", "Application Startup", "BeautyBookingApp", "Application started successfully.");
            }

        }

    }

    

}
