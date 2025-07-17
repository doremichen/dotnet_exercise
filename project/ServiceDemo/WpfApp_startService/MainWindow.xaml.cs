using System;
using System.Diagnostics;
using System.IO;
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

namespace WpfApp_startService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Process? backgroundProcess = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnStartService_Click(object sender, RoutedEventArgs e)
        {
            string exePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "workBin",
                "BackgroundConsoleApp.exe");

            Debug.WriteLine($"exePath: {exePath}");

            // start service logic here
            backgroundProcess = Process.Start(new ProcessStartInfo
            {
                FileName = exePath, // 改為你的實際路徑或使用相對路徑
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (backgroundProcess == null)
            {
                MessageBox.Show("啟動失敗", "Service Status", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"已啟動，PID = {backgroundProcess.Id}", "Service Status", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }

        private void BtnStopService_Click(object sender, RoutedEventArgs e)
        {
            // 停止服務的邏輯
            if (backgroundProcess != null && !backgroundProcess.HasExited)
            {
                backgroundProcess.Kill();
                backgroundProcess = null;
                MessageBox.Show("Service stopped successfully!", "Service Status", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("No service is running.", "Service Status", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }
    }
}