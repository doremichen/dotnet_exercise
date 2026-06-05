// ============================================================================
// Copyright (c) 2026 AdamChen. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for details.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ============================================================================

/*
 * Description: A simple system monitor application.
 * UI is responsible only for presentation. Monitoring is done by a separate process
 * which sends data via a named pipe.
 * Author: Adam Chen
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
        private readonly CancellationTokenSource _cts;
        private Process? _monitoringProcess;
        private NamedPipeClientStream? _clientStream;
        private readonly SemaphoreSlim _clientWriteSemaphore = new SemaphoreSlim(1, 1);

        public MainWindow()
        {
            InitializeComponent();

            _cts = new CancellationTokenSource();

            // KISS Principle: Don't perform synchronous IO or process startup in the constructor.
            // Delegate to a background task for a smooth UI experience and reliable debugging.
            Task.Run(async () =>
            {
                StartMonitoringService();
                await RunBidirectionalClientAsync(_cts.Token);
            });
        }

        private void StartMonitoringService()
        {
            Trace.WriteLine("[INFO] Starting StartMonitoringService...");
            try
            {
                if (_monitoringProcess != null && !_monitoringProcess.HasExited)
                {
                    Trace.WriteLine("[INFO] MonitoringService is already running.");
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
                    // Fallback: Development relative paths
                    Path.Combine(baseDir, "..", "..", "..", "MonitoringService", "bin", "Debug", "net8.0", "MonitoringService.exe"),
                    Path.Combine(baseDir, "..", "..", "..", "MonitoringService", "bin", "Debug", "net8.0", "MonitoringService.dll"),
                };

                string? found = candidates.FirstOrDefault(File.Exists);
                if (found == null)
                {
                    Trace.WriteLine("[ERROR] Startup failed: Could not find any MonitoringService files.");
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

                Trace.WriteLine($"[INFO] Determined path: {found}, Command: {psi.FileName} {psi.Arguments}");

                _monitoringProcess = new Process { StartInfo = psi };

                // Intercept service stdout and forward to WPF Trace
                _monitoringProcess.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Trace.WriteLine($"[SERVICE CONSOLE] {e.Data}");
                    }
                };
                _monitoringProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Trace.WriteLine($"[SERVICE ERROR CONSOLE] {e.Data}");
                    }
                };

                _monitoringProcess.Start();

                // Start asynchronous line reading
                _monitoringProcess.BeginOutputReadLine();
                _monitoringProcess.BeginErrorReadLine();

                Trace.WriteLine($"[SUCCESS] MonitoringService started. PID: {_monitoringProcess.Id}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[EXCEPTION] Error in StartMonitoringService: {ex.Message}");
            }
        }

        private void StopMonitoringService()
        {
            Trace.WriteLine("[INFO] Starting StopMonitoringService cleanup...");
            if (_monitoringProcess == null) return;

            try
            {
                _monitoringProcess.Refresh();
                if (!_monitoringProcess.HasExited)
                {
                    int pid = _monitoringProcess.Id;
                    Trace.WriteLine($"[INFO] MonitoringService (PID: {pid}) is still running. Terminating...");

                    // Use taskkill to ensure the process tree is cleaned up silently
                    using (var killer = new Process())
                    {
                        killer.StartInfo.FileName = "taskkill";
                        killer.StartInfo.Arguments = $"/F /PID {pid} /T";
                        killer.StartInfo.CreateNoWindow = true;
                        killer.StartInfo.UseShellExecute = false;
                        killer.Start();
                        killer.WaitForExit(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[INFO] Service stop process encountered an exception (ignorable): {ex.Message}");
            }
            finally
            {
                try { _monitoringProcess.Dispose(); } catch { }
                _monitoringProcess = null;
                Trace.WriteLine("[INFO] MonitoringService cleanup completed.");
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Trace.WriteLine("[INFO] MainWindow is closing, cleaning up resources...");
            _cts?.Cancel();
            StopMonitoringService();
            base.OnClosing(e);
        }

        private async Task RunBidirectionalClientAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    _clientStream = new NamedPipeClientStream(
                        ".",
                        PipeName,
                        PipeDirection.InOut,
                        PipeOptions.Asynchronous);

                    Trace.WriteLine("[INFO] Attempting to connect to Named Pipe service...");

                    // 5-second buffer for service initialization
                    await _clientStream.ConnectAsync(5000, ct);

                    if (!_clientStream.IsConnected)
                    {
                        Trace.WriteLine("[WARN] Failed to connect to service. Retrying in 1s...");
                        await Task.Delay(1000, ct);
                        continue;
                    }

                    await ClientReadLoopAsync(_clientStream, ct);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Trace.WriteLine($"[PIPE INFRASTRUCTURE] Connection encountered an exception: {ex.Message}");
                    await Task.Delay(1000, ct);
                }
                finally
                {
                    try { _clientStream?.Dispose(); _clientStream = null; } catch { }
                }
            }
        }

        private async Task ClientReadLoopAsync(NamedPipeClientStream client, CancellationToken ct)
        {
            Trace.WriteLine("[SUCCESS] Connected to service pipe. Starting data stream reading...");
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
                            Trace.WriteLine("[INFO] Service disconnected.");
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
                            Trace.WriteLine("[INFO] Service disconnected.");
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
                Trace.WriteLine($"[ERROR] ClientReadLoop encountered an error: {ex.Message}");
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
                ramText.Text = $"Memory: Used {usedMB:F0} MB / Total {data.TotalMemoryMB:F0} MB";

                diskInfoList.Items.Clear();
                if (data.DiskInfos != null)
                {
                    foreach (var d in data.DiskInfos)
                    {
                        double used = d.TotalGB - d.FreeGB;
                        double percent = d.TotalGB > 0 ? used / d.TotalGB * 100.0 : 0.0;
                        diskInfoList.Items.Add($"{d.Name}: Used {used:F1} GB / {d.TotalGB:F1} GB ({percent:F1}%)");
                    }
                }
            }
            catch (Exception ex)
            {
                cpuText.Text = $"UI Update Error: {ex.Message}";
            }
        }

        private async Task SendCommandAsync(string type, object payload)
        {
            Trace.WriteLine($"[COMMAND] Preparing to send command to service: {type}");
            try
            {
                if (_clientStream == null || !_clientStream.IsConnected) return;
                var env = new { Type = type, Payload = payload };
                var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(env));
                var len = BitConverter.GetBytes(bytes.Length);

                await _clientWriteSemaphore.WaitAsync();
                try
                {
                    if (_clientStream != null && _clientStream.IsConnected)
                    {
                        await _clientStream.WriteAsync(len, 0, 4);
                        await _clientStream.WriteAsync(bytes, 0, bytes.Length);
                        await _clientStream.FlushAsync();
                    }
                }
                finally
                {
                    _clientWriteSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[ERROR] SendCommandAsync failed: {ex.Message}");
            }
        }

        private void OptimizeMemory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                MessageBox.Show("Memory optimization has been executed.", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during optimization: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to close the application?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
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
