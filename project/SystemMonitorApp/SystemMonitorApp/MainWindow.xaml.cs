/*
 * Description: A simple system monitor application
 * UI is responsible only for presentation. Monitoring is done by a separate process
 * which sends data via a named pipe.
 * Author: Adam chen (adapted)
 * Date: 2025/07/16
 */
using MonitoringContracts;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;

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
        // lock for synchronizing writes to the pipe, since multiple UI actions could trigger commands
        private readonly SemaphoreSlim clientWriteSemaphore = new SemaphoreSlim(1, 1);


        public MainWindow()
        {
            InitializeComponent();
            // Try to start monitoring service process
            StartMonitoringService();
            // Start background task to connect to the service and handle bidirectional messages
            cts = new CancellationTokenSource();
            Task.Run(() => RunBidirectionalClientAsync(cts.Token));
        }



        private void StartMonitoringService()
        {
            try
            {
                if (monitoringProcess != null && !monitoringProcess.HasExited)
                {
                    Debug.WriteLine("[INFO] MonitoringService 已經在執行中。");
                    return;
                }

                var baseDir = AppContext.BaseDirectory;

                // Priority 1: Look in the bundled Services subfolder (produced by build integration)
                var servicesDir = Path.Combine(baseDir, "Services");
                var candidates = new[]
                {
                    Path.Combine(servicesDir, "MonitoringService.dll"),
                    Path.Combine(servicesDir, "MonitoringService.exe"),
                    // Fallback: development paths for running from Visual Studio
                    Path.Combine(baseDir, "..", "..", "..", "MonitoringService", "bin", "Debug", "net8.0", "MonitoringService.dll"),
                    Path.Combine(baseDir, "..", "..", "..", "MonitoringService", "bin", "Debug", "net8.0", "MonitoringService.exe"),
                };

                string? found = candidates.FirstOrDefault(File.Exists);
                if (found == null)
                {
                    Debug.WriteLine("[ERROR] 啟動失敗：找不到任何 MonitoringService 的檔案路徑。");
                    return; // nothing to start
                }

                string workingDir = Path.GetDirectoryName(found) ?? baseDir;
                string arguments = string.Empty;

                // Prefer running dll via dotnet to ensure proper assembly resolution
                if (found.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || found.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = '"' + found + '"' + (string.IsNullOrWhiteSpace(arguments) ? string.Empty : " " + arguments),
                        WorkingDirectory = workingDir,
                        UseShellExecute = false,
                        CreateNoWindow = true, // run hidden

                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    Debug.WriteLine($"[INFO] 嘗試啟動 Service，路徑: {found}");
                    monitoringProcess = new Process { StartInfo = psi };
                    // Capture output for debugging purposes;
                    // in production consider logging to a file instead
                    monitoringProcess.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            Debug.WriteLine($"[SERVICE CONSOLE] {e.Data}");
                        }
                    };
                    monitoringProcess.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            Debug.WriteLine($"[SERVICE ERROR CONSOLE] {e.Data}");
                        }
                    };

                    monitoringProcess.Start();

                    // Begin async read of output streams
                    monitoringProcess.BeginOutputReadLine();
                    monitoringProcess.BeginErrorReadLine();

                    // 【新增：確認啟動成功/失敗的 Log 機制】
                    if (monitoringProcess != null)
                    {
                        // wait a short moment to see if the process exits immediately with an error
                        bool exitedEarly = monitoringProcess.WaitForExit(500);
                        if (exitedEarly && monitoringProcess.ExitCode != 0)
                        {
                            Debug.WriteLine($"[ERROR] Service 啟動後異常退出，Exit Code: {monitoringProcess.ExitCode}");
                        }
                        else
                        {
                            Debug.WriteLine($"[SUCCESS] MonitoringService 成功啟動。PID: {monitoringProcess.Id}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("[ERROR] Process.Start 回傳 null，Service 啟動失敗。");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EXCEPTION] StartMonitoringService 發生異常: {ex.Message}");
            }
        }

        private void StopMonitoringService()
        {
            Debug.WriteLine("[INFO] 嘗試停止 MonitoringService...");
            try
            {
                if (monitoringProcess != null && !monitoringProcess.HasExited)
                {
                    Debug.WriteLine($"[INFO] 正在停止 MonitoringService (PID: {monitoringProcess.Id})...");
                    monitoringProcess.Kill(true);
                    monitoringProcess.Dispose();
                    monitoringProcess = null;
                    Debug.WriteLine("[SUCCESS] MonitoringService 已成功強制終止。");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EXCEPTION] StopMonitoringService 失敗: {ex.Message}");
            }
        }


        //
        // Assume the UI has a proper Close button that calls CloseApp_Click,
        // which in turn calls this OnClosing method to ensure all resources are cleaned up properly.
        //
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Debug.WriteLine("[INFO] MainWindow 正在關閉，開始清理資源...");

            // 1. Stop the background client task by cancelling its token, which will cause it to exit gracefully
            cts?.Cancel();

            // 2. Attempt to close the client stream if it's still open
            StopMonitoringService();

            base.OnClosing(e);
        }

        private async Task RunBidirectionalClientAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    clientStream = new NamedPipeClientStream(
                        ".",
                        PipeName,
                        PipeDirection.InOut,
                        PipeOptions.Asynchronous);
                    Debug.WriteLine("[INFO] 嘗試連接到 Named Pipe 服務...");
                    await clientStream.ConnectAsync(3000, ct);

                    if (!clientStream.IsConnected)
                    {
                        Debug.WriteLine("[WARN] 無法連接到服務，稍後重試...");
                        await Task.Delay(1000, ct);
                        continue;
                    }

                    // start the async read loop directly (no extra Task.Run for IO-bound async methods)
                    await ClientReadLoopAsync(clientStream, ct);
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


        // client read loop: receives Envelope messages from service and acts accordingly
        private async Task ClientReadLoopAsync(NamedPipeClientStream client, CancellationToken ct)
        {
            Debug.WriteLine("[INFO] 已成功連接到服務，開始讀取數據...");
            var lenBuf = new byte[4];
            try
            {
                while (client.IsConnected && !ct.IsCancellationRequested)
                {
                    int got = 0;
                    while (got < 4)
                    {
                        int r = await client.ReadAsync(lenBuf.AsMemory(got, 4 - got), ct);
                        if (r == 0)
                        {
                            Debug.WriteLine("[INFO] 服務已斷開連接。");
                            return; // disconnected
                        }
                        got += r;
                    }
                    int msgLen = BitConverter.ToInt32(lenBuf, 0);
                    if (msgLen <= 0) continue;
                    var payload = new byte[msgLen];
                    int rec = 0;
                    while (rec < msgLen)
                    {
                        int r = await client.ReadAsync(payload.AsMemory(rec, msgLen - rec), ct);
                        if (r == 0)
                        {
                            Debug.WriteLine("[INFO] 服務已斷開連接。");
                            return; // disconnected
                        }
                        rec += r;
                    }
                    var env = JsonSerializer.Deserialize<Envelope>(Encoding.UTF8.GetString(payload));
                    if (env != null)
                    {
                        if (env.Type == "MonitorData")
                        {
                            var md = env.Payload.Deserialize<MonitorData>();
                            if (md != null)
                                await Dispatcher.BeginInvoke(new Action(() => ApplyMonitorDataToUi(md)));
                        }
                        else if (env.Type == "ProcessList")
                        {
                            var procs = env.Payload.Deserialize<ProcessInfo[]>();
                            if (procs != null)
                                // update process list on UI thread; in a real app consider using ObservableCollection
                                // and data binding for better performance with large lists
                                await Dispatcher.BeginInvoke(new Action(() => processListView.ItemsSource = procs));
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


        // send a command envelope to service
        private async Task SendCommandAsync(string type, object payload)
        {
            Debug.WriteLine($"Sending command to service: {type}");
            try
            {
                if (clientStream == null || !clientStream.IsConnected) return;
                var env = new { Type = type, Payload = payload };
                var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(env));
                var len = BitConverter.GetBytes(bytes.Length);
                // ensure only one write at a time to avoid interleaving messages
                // if user clicks multiple buttons quickly
                await clientWriteSemaphore.WaitAsync();
                try
                {
                    if (clientStream != null && clientStream.IsConnected)
                    {
                        await clientStream.WriteAsync(len, 0, 4);
                        await clientStream.WriteAsync(bytes, 0, bytes.Length);
                        await clientStream.FlushAsync();
                    }
                }
                finally
                {
                    clientWriteSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SendCommandAsync failed: {ex.Message}");
            }
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
                // OnClosing will handle cleanup of resources and stopping the monitoring service
                this.Close();
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