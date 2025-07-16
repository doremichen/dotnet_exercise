/**
 *  Description: A simple system monitor application 
 *              that displays CPU and memory usage, and allows memory optimization.
 *  Author: Adam chen
 *  Date: 2025/07/16
 */
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Windows;

namespace SystemMonitorApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Placeholder for CPU counter, replace with actual implementation
        private PerformanceCounter cpucounter;
        // Placeholder for memory counter, replace with actual implementation
        private PerformanceCounter memorycounter;
        // dispatch timer for updating UI
        private System.Windows.Threading.DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            // cpucounter is a placeholder for actual CPU counter logic
            cpucounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            // memorycounter is a placeholder for actual memory counter logic
            memorycounter = new PerformanceCounter("Memory", "Available MBytes");

            // Initialize the timer to update the UI every second
            timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
            timer.Start();

        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            // Update CPU usage
            try
            {
                double cpuUsage = cpucounter.NextValue();
                cpuBar.Value = (int)cpuUsage;
                cpuText.Text = $"CPU Usage: {cpuUsage:F2}%";
            }
            catch (Exception ex)
            {
                cpuText.Text = $"Error retrieving CPU usage: {ex.Message}";
            }

            // Update memory usage
            try
            {
                float availableMB = memorycounter.NextValue();
                float totalMB = GetTotalMemoryInMBytes();
                float usedMB = totalMB - availableMB;
                float ramPercent = usedMB / totalMB * 100;

                ramBar.Value = ramPercent;
                ramText.Text = $"記憶體: 已用 {usedMB:F0} MB / 共 {totalMB:F0} MB";
            }
            catch (Exception ex)
            {
                ramText.Text = $"Error retrieving memory usage: {ex.Message}";
            }

            diskInfoList.Items.Clear();
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    double total = drive.TotalSize / (1024.0 * 1024 * 1024);
                    double free = drive.TotalFreeSpace / (1024.0 * 1024 * 1024);
                    double used = total - free;
                    double percent = used / total * 100;
                    diskInfoList.Items.Add($"{drive.Name}：已用 {used:F1} GB / {total:F1} GB ({percent:F1}%)");
                }
            }

        }

        private float GetTotalMemoryInMBytes()
        {
            var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
            foreach (var obj in searcher.Get())
            {
                if (obj["TotalPhysicalMemory"] is ulong totalMemory)
                {
                    return totalMemory / 1024f / 1024f;
                }
            }
            return 0;
        }

        private void OptimizeMemory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GC.Collect();                  // 強制進行垃圾回收
                GC.WaitForPendingFinalizers(); // 等待終結器執行完成
                GC.Collect();                  // 再次收集，以確保清乾淨

                MessageBox.Show("記憶體最佳化已執行。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"最佳化時發生錯誤：{ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}