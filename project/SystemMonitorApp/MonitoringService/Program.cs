using MonitoringContracts;
using System.Diagnostics;
using System.IO.Pipes;
using System.Management;
using System.Text;
using System.Text.Json;

namespace MonitoringService
{
    /*
     * Bidirectional monitoring service:
     * - sends periodic MonitorData messages to UI
     * - accepts command messages from UI and responds
     */
    internal class Program
    {
        private const string PipeName = "SystemMonitorPipe";
        // PerformanceCounter can throw exceptions if the underlying counters
        // are unavailable or if permissions are insufficient,
        private static PerformanceCounter? cpuCounter;
        private static PerformanceCounter? memoryCounter;


        private static readonly SemaphoreSlim serverWriteSemaphore = new SemaphoreSlim(1, 1);

        static void Main(string[] args)
        {
            Debug.WriteLine("[INIT] MonitoringService are initializing counter...");
            InitializeCounters();

            while (true)
            {
                // Allow multiple server instances to be created (use system max)
                // to avoid "all pipe instances are in use" errors
                using (var server = new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous))
                {
                    try
                    {
                        Debug.WriteLine("Waiting for UI client to connect...");
                        server.WaitForConnection();
                        Debug.WriteLine("UI connected, starting worker tasks...");

                        var cts = new CancellationTokenSource();
                        int intervalMs = 1000;

                        // writer task: periodically send MonitorData and also send responses to commands
                        var writer = WriterLoopAsync(server, () => intervalMs, cts.Token);
                        var reader = ReaderLoopAsync(server, (cmd, payload) =>
                        {
                            HandleCommand(cmd, payload, server, ref intervalMs);
                        }, cts.Token);

                        // wait for either task to fail (e.g. due to disconnection),
                        // then cancel the other and restart the loop to wait for a new connection
                        Task.WaitAll(new[] { writer, reader }, Timeout.Infinite);

                        cts.Cancel();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Pipe accept/connection failed: {ex}");
                    }
                    finally
                    {
                        try { if (server.IsConnected) server.Disconnect(); } catch { }
                    }
                }

                Thread.Sleep(500);
            }
        }

        private static void InitializeCounters()
        {
            try
            {
                //
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                cpuCounter.NextValue();
                Debug.WriteLine("[SUCCESS] CPU 效能計數器初始化成功。");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CRITICAL] CPU 計數器初始化失敗 (可能權限不足): {ex.Message}");
            }

            try
            {
                memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
                memoryCounter.NextValue();
                Debug.WriteLine("[SUCCESS] Memory 效能計數器初始化成功。");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CRITICAL] Memory 計數器初始化失敗: {ex.Message}");
            }
        }


        private static async Task WriterLoopAsync(PipeStream server, Func<int> getIntervalMs, CancellationToken ct)
        {
            Debug.WriteLine("[WRITER] WriterLoop started.");
            try
            {
                while (server.IsConnected && !ct.IsCancellationRequested)
                {
                    double cpuVal = -1;
                    float memVal = -1;

                    try { if (cpuCounter != null) cpuVal = cpuCounter.NextValue(); }
                    catch (Exception ex) { Debug.WriteLine($"[DATA ERROR] 讀取 CPU 失敗: {ex.Message}"); }

                    try { if (memoryCounter != null) memVal = memoryCounter.NextValue(); }
                    catch (Exception ex) { Debug.WriteLine($"[DATA ERROR] 讀取 Memory 失敗: {ex.Message}"); }


                    var data = new MonitorData
                    {
                        CpuUsage = cpuVal,
                        AvailableMemoryMB = memVal,
                        TotalMemoryMB = GetTotalMemoryInMBytes(),
                        DiskInfos = GetDiskInfos()
                    };

                    var envelope = new Envelope { Type = "MonitorData", Payload = JsonSerializer.SerializeToElement(data) };
                    var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));
                    var len = BitConverter.GetBytes(bytes.Length);

                    await serverWriteSemaphore.WaitAsync(ct);
                    try
                    {
                        if (server.IsConnected)
                        {
                            await server.WriteAsync(len, 0, len.Length, ct);
                            await server.WriteAsync(bytes, 0, bytes.Length, ct);
                            await server.FlushAsync(ct);
                            Debug.WriteLine($"[SEND] 已發送 MonitorData, 長度: {bytes.Length} bytes. CPU: {cpuVal:F1}%, RAM Free: {memVal}MB");

                        }
                    }
                    finally
                    {
                        serverWriteSemaphore.Release();
                    }


                    await Task.Delay(getIntervalMs(), ct);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WriterLoop error: {ex.Message}");
            }
        }

        private static async Task ReaderLoopAsync(PipeStream server, Action<string, JsonElement> onCommand, CancellationToken ct)
        {
            Debug.WriteLine("[READER] ReaderLoop started.");
            var lenBuf = new byte[4];
            try
            {
                while (server.IsConnected && !ct.IsCancellationRequested)
                {
                    int got = 0;
                    while (got < 4)
                    {
                        int r = await server.ReadAsync(lenBuf.AsMemory(got, 4 - got), ct);
                        if (r == 0) return; // disconnected
                        got += r;
                    }
                    int msgLen = BitConverter.ToInt32(lenBuf, 0);
                    if (msgLen <= 0) continue;
                    var payload = new byte[msgLen];
                    int rec = 0;
                    while (rec < msgLen)
                    {
                        int r = await server.ReadAsync(payload.AsMemory(rec, msgLen - rec), ct);
                        if (r == 0) return; // disconnected
                        rec += r;
                    }

                    try
                    {
                        var doc = JsonDocument.Parse(payload);
                        if (doc.RootElement.TryGetProperty("Type", out var t))
                        {
                            string type = t.GetString() ?? string.Empty;
                            if (doc.RootElement.TryGetProperty("Payload", out var p))
                            {
                                Console.WriteLine($"[RECEIVE] 收到 Client 指令: {type}");
                                onCommand(type, p);
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        Debug.WriteLine($"[JSON ERROR] 解析 Client 訊息失敗: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                Debug.WriteLine($"ReaderLoop cancelled: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ReaderLoop error: {ex.Message}");
            }
        }

        private static void HandleCommand(string cmd, JsonElement payload, PipeStream server, ref int intervalMs)
        {
            Debug.WriteLine($"[COMMAND] Handling command: {cmd}");
            try
            {
                if (cmd == "GetProcessList")
                {
                    var procs = Process.GetProcesses()
                        .Select(p => new ProcessInfo { Id = p.Id, ProcessName = p.ProcessName, WorkingSetMB = p.WorkingSet64 / 1024.0 / 1024.0 })
                        .OrderByDescending(x => x.WorkingSetMB)
                        .Take(30)
                        .ToArray();
                    var env = new Envelope { Type = "ProcessList", Payload = JsonSerializer.SerializeToElement(procs) };
                    SendEnvelope(server, env);
                }
                else if (cmd == "TriggerGC")
                {
                    GC.Collect();
                    var env = new Envelope { Type = "CommandAck", Payload = JsonSerializer.SerializeToElement(new { Command = "TriggerGC", Status = "OK" }) };
                    SendEnvelope(server, env);
                }
                else if (cmd == "SetIntervalSeconds")
                {
                    if (payload.TryGetProperty("Seconds", out var s) && s.TryGetInt32(out var seconds))
                    {
                        intervalMs = Math.Max(200, seconds * 1000);
                        var env = new Envelope { Type = "CommandAck", Payload = JsonSerializer.SerializeToElement(new { Command = "SetIntervalSeconds", Status = "OK", Seconds = seconds }) };
                        SendEnvelope(server, env);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HandleCommand error: {ex.Message}");
            }
        }

        private static void SendEnvelope(PipeStream server, Envelope env)
        {
            Debug.WriteLine($"[SEND] Sending envelope of type: {env.Type}");
            try
            {
                var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(env));
                var len = BitConverter.GetBytes(bytes.Length);
                serverWriteSemaphore.Wait();
                try
                {
                    if (server.IsConnected)
                    {
                        server.Write(len, 0, len.Length);
                        server.Write(bytes, 0, bytes.Length);
                        server.Flush();
                    }
                }
                finally
                {
                    serverWriteSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SendEnvelope failed: {ex.Message}");
            }
        }

        private static float GetTotalMemoryInMBytes()
        {
            Debug.WriteLine("[INFO] Retrieving total physical memory via WMI...");
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        if (obj["TotalPhysicalMemory"] is ulong totalMemory)
                        {
                            return totalMemory / 1024f / 1024f;
                        }
                    }
                }
            }
            catch { }
            return 0;
        }

        private static DiskInfo[] GetDiskInfos()
        {
            try
            {
                return DriveInfo.GetDrives()
                    .Where(d => d.IsReady)
                    .Select(d => new DiskInfo
                    {
                        Name = d.Name,
                        TotalGB = d.TotalSize / (1024.0 * 1024 * 1024),
                        FreeGB = d.TotalFreeSpace / (1024.0 * 1024 * 1024)
                    }).ToArray();
            }
            catch
            {
                return Array.Empty<DiskInfo>();
            }
        }
    }
}
