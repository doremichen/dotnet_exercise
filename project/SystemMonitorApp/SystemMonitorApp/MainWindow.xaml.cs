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

namespace SystemMonitorApp
{
    public partial class MainWindow : Window
    {
        private const string PipeName = "SystemMonitorPipe";
        private CancellationTokenSource? cts;
        private Process? monitoringProcess;
        private NamedPipeClientStream? clientStream;
        private readonly SemaphoreSlim clientWriteSemaphore = new SemaphoreSlim(1, 1);

        public MainWindow()
        {
            InitializeComponent();

            cts = new CancellationTokenSource();

            // KISS 原則：不要在建構子同步做 IO/進程啟動。
            // 直接丟給背景 Task，順序明朗，UI 秒開不卡死，偵錯管道有充裕時間接管！
            Task.Run(async () =>
            {
                //#if !DEBUG
                //                // 讓附加進程的提示視窗在背景執行緒跳出，完全不卡死 UI 渲染
                //                System.Windows.MessageBox.Show("請先去 Visual Studio 附加此進程，再點擊確定開始啟動 Service！");
                //#endif
                // 先啟動 Service，再連接 Pipe
                StartMonitoringService();
                await RunBidirectionalClientAsync(cts.Token);
            });
        }

        private void StartMonitoringService()
        {
            // 改用 Trace.WriteLine，WPF 視窗程式不論何時附加，VS Debug 視窗都看得到！
            Trace.WriteLine("[INFO] 開始執行 StartMonitoringService...");
            try
            {
                if (monitoringProcess != null && !monitoringProcess.HasExited)
                {
                    Trace.WriteLine("[INFO] MonitoringService 已經在執行中。");
                    return;
                }

                var baseDir = AppContext.BaseDirectory;
                var servicesDir = Path.Combine(baseDir, "Services");

                var candidates = new[]
                {
                    Path.Combine(servicesDir, "MonitoringService.exe"),
                    Path.Combine(servicesDir, "MonitoringService.dll"),
                    Path.Combine(baseDir, "MonitoringService.exe"),
                    Path.Combine(baseDir, "MonitoringService.dll"),
                    // Fallback: 開發環境相對路徑
                    Path.Combine(baseDir, "..", "..", "..", "MonitoringService", "bin", "Debug", "net8.0", "MonitoringService.exe"),
                    Path.Combine(baseDir, "..", "..", "..", "MonitoringService", "bin", "Debug", "net8.0", "MonitoringService.dll"),
                };

                string? found = candidates.FirstOrDefault(File.Exists);
                if (found == null)
                {
                    Trace.WriteLine("[ERROR] 啟動失敗：找不到任何 MonitoringService 的檔案路徑。");
                    return;
                }

                string workingDir = Path.GetDirectoryName(found) ?? baseDir;
                string ext = Path.GetExtension(found).ToLower();

                var psi = new ProcessStartInfo
                {
                    WorkingDirectory = workingDir,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                if (ext == ".exe")
                {
                    psi.FileName = found;
                    psi.Arguments = string.Empty;
                }
                else
                {
                    psi.FileName = "dotnet";
                    psi.Arguments = $"\"{found}\"";
                }

                Trace.WriteLine($"[INFO] 最終判定路徑: {found}，啟動命令: {psi.FileName} {psi.Arguments}");

                monitoringProcess = new Process { StartInfo = psi };

                // 攔截 Service 的 stdout，轉發到 WPF 的 Trace 管道
                monitoringProcess.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Trace.WriteLine($"[SERVICE CONSOLE] {e.Data}");
                    }
                };
                monitoringProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Trace.WriteLine($"[SERVICE ERROR CONSOLE] {e.Data}");
                    }
                };

                monitoringProcess.Start();

                // 啟動非同步序列讀取
                monitoringProcess.BeginOutputReadLine();
                monitoringProcess.BeginErrorReadLine();

                Trace.WriteLine($"[SUCCESS] MonitoringService 已喚醒。PID: {monitoringProcess.Id}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[EXCEPTION] StartMonitoringService 發生異常: {ex.Message}");
            }
        }

        private void StopMonitoringService()
        {
            Trace.WriteLine("[INFO] 開始執行 StopMonitoringService 清理資源...");
            if (monitoringProcess == null) return;

            try
            {
                monitoringProcess.Refresh();
                if (!monitoringProcess.HasExited)
                {
                    int pid = monitoringProcess.Id;
                    Trace.WriteLine($"[INFO] 偵測到 MonitoringService (PID: {pid}) 仍活著，改用作業系統級指令強制終止...");

                    // 呼叫 Windows 內建的 taskkill 命令來執行，由 Windows 自行默默清理，.NET 就不會噴任何 Exception
                    using (var killer = new System.Diagnostics.Process())
                    {
                        killer.StartInfo.FileName = "taskkill";
                        killer.StartInfo.Arguments = $"/F /PID {pid} /T"; // /T 代表連同子行程一起殺
                        killer.StartInfo.CreateNoWindow = true;
                        killer.StartInfo.UseShellExecute = false;
                        killer.Start();
                        killer.WaitForExit(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[INFO] 停止服務過程正常放行: {ex.Message}");
            }
            finally
            {
                try { monitoringProcess.Dispose(); } catch { }
                monitoringProcess = null;
                Trace.WriteLine("[INFO] MonitoringService 資源清理完畢。");
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Trace.WriteLine("[INFO] MainWindow 正在關閉，開始清理資源...");
            cts?.Cancel();
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

                    Trace.WriteLine("[INFO] 正在嘗試連接到 Named Pipe 服務...");

                    // 這裡給予 5 秒連線緩衝，避免 Service 初始化太慢
                    await clientStream.ConnectAsync(5000, ct);

                    if (!clientStream.IsConnected)
                    {
                        Trace.WriteLine("[WARN] 無法連接到服務，1秒後重試...");
                        await Task.Delay(1000, ct);
                        continue;
                    }

                    await ClientReadLoopAsync(clientStream, ct);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    // 這裡可以將底層拋出的 Exception 印出來看看
                    Trace.WriteLine($"[PIPE INFRASTRUCTURE] 連線過程遭遇異常: {ex.Message}");
                    await Task.Delay(1000, ct);
                }
                finally
                {
                    try { clientStream?.Dispose(); clientStream = null; } catch { }
                }
            }
        }

        private async Task ClientReadLoopAsync(NamedPipeClientStream client, CancellationToken ct)
        {
            Trace.WriteLine("[SUCCESS] 已成功連接到服務管線，開始讀取數據串流...");
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
                            Trace.WriteLine("[INFO] 服務已主動斷開連接。");
                            return;
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
                            Trace.WriteLine("[INFO] 服務已主動斷開連接。");
                            return;
                        }
                        rec += r;
                    }

                    var jsonStr = Encoding.UTF8.GetString(payload);
                    var env = JsonSerializer.Deserialize<Envelope>(jsonStr);
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
                                await Dispatcher.BeginInvoke(new Action(() => processListView.ItemsSource = procs));
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Trace.WriteLine($"[ERROR] ClientReadLoop 發生錯誤: {ex.Message}");
            }
        }

        private void ApplyMonitorDataToUi(MonitorData data)
        {
            try
            {
                cpuBar.Value = (int)Math.Round(data.CpuUsage);
                cpuText.Text = $"CPU 使用率: {data.CpuUsage:F2}%";

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
                cpuText.Text = $"UI 更新錯誤: {ex.Message}";
            }
        }

        private async Task SendCommandAsync(string type, object payload)
        {
            Trace.WriteLine($"[COMMAND] 準備發送指令至 Service: {type}");
            try
            {
                if (clientStream == null || !clientStream.IsConnected) return;
                var env = new { Type = type, Payload = payload };
                var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(env));
                var len = BitConverter.GetBytes(bytes.Length);

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
                Trace.WriteLine($"[ERROR] SendCommandAsync 失敗: {ex.Message}");
            }
        }

        private void OptimizeMemory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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
            if (MessageBox.Show("確定要關閉應用程式嗎？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
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