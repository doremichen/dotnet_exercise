/*
 *  Description: A simple system monitor application
 *               UI is responsible only for presentation. Monitoring is done by a separate process
 *               which sends data via a named pipe.
 *  Author: Adam chen (adapted)
 *  Date: 2025/07/16
 */
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;
using System.Linq;

namespace SystemMonitorApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string PipeName = "SystemMonitorPipe";
        private CancellationTokenSource? cts;
        private Process? monitoringProcess;
        private NamedPipeClientStream? clientStream;
        private readonly object clientWriteLock = new object();


        private void StartMonitoringService()
        {
            try
            {
                if (monitoringProcess != null && !monitoringProcess.HasExited)
                    return;

                var baseDir = AppContext.BaseDirectory;
                var candidates = new[]
                {
                    Path.Combine(baseDir, "MonitoringService.exe"),
                    Path.Combine(baseDir, "MonitoringService.dll"),
                    Path.Combine(baseDir, "..", "..", "..", "MonitoringService", "bin", "Debug", "net8.0", "MonitoringService.exe"),
                    Path.Combine(baseDir, "..", "..", "..", "MonitoringService", "bin", "Debug", "net8.0", "MonitoringService.dll"),
                    Path.Combine(baseDir, "..", "..", "..", "..", "MonitoringService", "bin", "Debug", "net8.0", "MonitoringService.exe"),
                    Path.Combine(baseDir, "..", "..", "..", "..", "MonitoringService", "bin", "Debug", "net8.0", "MonitoringService.dll")
                };

                string? found = candidates.FirstOrDefault(File.Exists);
                if (found == null)
                    return; // nothing to start

                // Example: start without elevation by default to avoid UAC and permission issues
                bool startElevated = false; // set to true when you explicitly want UAC elevation
                string workingDir = Path.GetDirectoryName(found) ?? baseDir;
                string arguments = string.Empty; // customize args for the monitoring service if needed

                if (found.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    var psi = new ProcessStartInfo(found)
                    {
                        WorkingDirectory = workingDir,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Arguments = arguments
                    };
                    monitoringProcess = Process.Start(psi);
                }
                else
                {
                    // start the dll via dotnet without elevation
                    var psi = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = '"' + found + '"' + (string.IsNullOrWhiteSpace(arguments) ? string.Empty : " " + arguments),
                        WorkingDirectory = workingDir,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    monitoringProcess = Process.Start(psi);
                }
            }
            catch (Exception ex)
            {
                // log or ignore; on elevation cancel the user may have declined UAC
                Debug.WriteLine($"StartMonitoringService failed: {ex.Message}");
            }
        }

        private void StopMonitoringService()
        {
            try
            {
                if (monitoringProcess != null && !monitoringProcess.HasExited)
                {
                    monitoringProcess.Kill(true);
                    monitoringProcess.Dispose();
                }
            }
            catch { }
        }

        public MainWindow()
        {
            InitializeComponent();
            // Try to start monitoring service process
            StartMonitoringService();
            // Start background task to connect to the service and handle bidirectional messages
            cts = new CancellationTokenSource();
            Task.Run(() => RunBidirectionalClientAsync(cts.Token));
        }

        private async Task RunBidirectionalClientAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    clientStream = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                    var connectTask = clientStream.ConnectAsync(3000, ct);
                    await connectTask;
                    if (!clientStream.IsConnected)
                    {
                        await Task.Delay(1000, ct);
                        continue;
                    }

                    var readTask = Task.Run(() => ClientReadLoopAsync(clientStream, ct));
                    // keep write access via helper SendCommandAsync
                    await readTask;
                }
                catch (OperationCanceledException) { break; }
                catch (Exception)
                {
                    await Task.Delay(1000, ct);
                }
                finally
                {
                    try { clientStream?.Dispose(); clientStream = null; } catch { }
                }
            }
        }

        private void ApplyMonitorDataToUi(MonitorData data)
        {
            try
            {
                cpuBar.Value = (int)Math.Round(data.CpuUsage);
                cpuText.Text = $"CPU Usage: {data.CpuUsage:F2}%";

                float usedMB = data.TotalMemoryMB - data.AvailableMemoryMB;
                float ramPercent = data.TotalMemoryMB > 0 ? usedMB / data.TotalMemoryMB * 100f : 0f;
                ramBar.Value = ramPercent;
                ramText.Text = $"記憶體: 已用 {usedMB:F0} MB / 共 {data.TotalMemoryMB:F0} MB";

                diskInfoList.Items.Clear();
                if (data.DiskInfos != null)
                {
                    foreach (var d in data.DiskInfos)
                    {
                        double used = d.TotalGB - d.FreeGB;
                        double percent = d.TotalGB > 0 ? used / d.TotalGB * 100.0 : 0.0;
                        diskInfoList.Items.Add($"{d.Name}：已用 {used:F1} GB / {d.TotalGB:F1} GB ({percent:F1}%)");
                    }
                }
            }
            catch (Exception ex)
            {
                // keep UI stable on errors
                cpuText.Text = $"UI update error: {ex.Message}";
            }
        }

        // client read loop: receives Envelope messages from service and acts accordingly
        private async Task ClientReadLoopAsync(NamedPipeClientStream client, CancellationToken ct)
        {
            var lenBuf = new byte[4];
            try
            {
                while (client.IsConnected && !ct.IsCancellationRequested)
                {
                    int got = 0;
                    while (got < 4)
                    {
                        int r = await client.ReadAsync(lenBuf.AsMemory(got, 4 - got), ct);
                        if (r == 0) return; // disconnected
                        got += r;
                    }
                    int msgLen = BitConverter.ToInt32(lenBuf, 0);
                    if (msgLen <= 0) continue;
                    var payload = new byte[msgLen];
                    int rec = 0;
                    while (rec < msgLen)
                    {
                        int r = await client.ReadAsync(payload.AsMemory(rec, msgLen - rec), ct);
                        if (r == 0) return;
                        rec += r;
                    }
                    var env = JsonSerializer.Deserialize<Envelope>(Encoding.UTF8.GetString(payload));
                    if (env != null)
                    {
                        if (env.Type == "MonitorData")
                        {
                            var md = env.Payload.Deserialize<MonitorData>();
                            if (md != null)
                                Dispatcher.BeginInvoke(new Action(() => ApplyMonitorDataToUi(md)));
                        }
                        else if (env.Type == "ProcessList")
                        {
                            var procs = env.Payload.Deserialize<ProcessInfo[]>();
                            if (procs != null)
                                Dispatcher.BeginInvoke(new Action(() => processListView.ItemsSource = procs));
                        }
                        else if (env.Type == "CommandAck")
                        {
                            // handle acknowledgements if desired
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.WriteLine($"ClientReadLoop error: {ex.Message}");
            }
        }

        private float GetTotalMemoryInMBytes()
        {
            // kept for compatibility if needed elsewhere in the UI
            try
            {
                var searcher = new System.Management.ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
                foreach (var obj in searcher.Get())
                {
                    if (obj["TotalPhysicalMemory"] is ulong totalMemory)
                    {
                        return totalMemory / 1024f / 1024f;
                    }
                }
            }
            catch { }
            return 0;
        }

        private void OptimizeMemory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // local optimization remains as a UI action; monitoring process is read-only in this version
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                MessageBox.Show("記憶體最佳化已執行。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"最佳化時發生錯誤：{ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("確定要關閉應用程式嗎？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // stop background client
                cts?.Cancel();
                // stop the monitoring process if we started it
                StopMonitoringService();
                Application.Current.Shutdown();
            }
        }

        private class MonitorData
        {
            public double CpuUsage { get; set; }
            public float AvailableMemoryMB { get; set; }
            public float TotalMemoryMB { get; set; }
            public DiskInfo[]? DiskInfos { get; set; }
        }

        private class DiskInfo
        {
            public string Name { get; set; } = string.Empty;
            public double TotalGB { get; set; }
            public double FreeGB { get; set; }
        }

        private class Envelope { public string Type { get; set; } public JsonElement Payload { get; set; } }
        private class ProcessInfo { public int Id { get; set; } public string ProcessName { get; set; } public double WorkingSetMB { get; set; } }

        // send a command envelope to service
        private async Task SendCommandAsync(string type, object payload)
        {
            try
            {
                if (clientStream == null || !clientStream.IsConnected) return;
                var env = new { Type = type, Payload = payload };
                var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(env));
                var len = BitConverter.GetBytes(bytes.Length);
                lock (clientWriteLock)
                {
                    clientStream.Write(len, 0, 4);
                    clientStream.Write(bytes, 0, bytes.Length);
                    clientStream.Flush();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SendCommandAsync failed: {ex.Message}");
            }
        }

        private void TriggerServiceGC_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => SendCommandAsync("TriggerGC", new { }));
        }

        private void RequestProcessList_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => SendCommandAsync("GetProcessList", new { }));
        }

        private void IntervalCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (intervalCombo.SelectedItem is System.Windows.Controls.ComboBoxItem item && int.TryParse(item.Content.ToString(), out var sec))
            {
                Task.Run(() => SendCommandAsync("SetIntervalSeconds", new { Seconds = sec }));
            }
        }
    }
}
