using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Management;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using MonitoringContracts;

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
        private static PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private static PerformanceCounter memoryCounter = new PerformanceCounter("Memory", "Available MBytes");

        static void Main(string[] args)
        {
            Console.WriteLine("MonitoringService started.");

            while (true)
            {
                // Allow multiple server instances to be created (use system max) to avoid "all pipe instances are in use" errors
                using (var server = new NamedPipeServerStream(PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
                {
                    try
                    {
                        Console.WriteLine("Waiting for UI client to connect...");
                        server.WaitForConnection();
                        Console.WriteLine("UI connected, starting worker tasks...");

                        var cts = new CancellationTokenSource();
                        var writeLock = new object();
                        int intervalMs = 1000;

                        // writer task: periodically send MonitorData and also send responses to commands
                        var writer = Task.Run(() => WriterLoopAsync(server, writeLock, () => intervalMs, cts.Token));
                        // reader task: receive commands from client
                        var reader = Task.Run(() => ReaderLoopAsync(server, writeLock, (cmd, payload) => HandleCommand(cmd, payload, server, writeLock, ref intervalMs), cts.Token));

                        Task.WaitAny(new[] { writer, reader });
                        // cancel the other
                        cts.Cancel();
                        Task.WaitAll(new[] { writer, reader }, 2000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Pipe accept/connection failed: {ex}");
                    }
                    finally
                    {
                        try { if (server.IsConnected) server.Disconnect(); } catch { }
                    }
                }

                Thread.Sleep(500);
            }
        }

        private static async Task WriterLoopAsync(PipeStream server, object writeLock, Func<int> getIntervalMs, CancellationToken ct)
        {
            try
            {
                while (server.IsConnected && !ct.IsCancellationRequested)
                {
                    var data = new MonitorData
                    {
                        CpuUsage = cpuCounter.NextValue(),
                        AvailableMemoryMB = memoryCounter.NextValue(),
                        TotalMemoryMB = GetTotalMemoryInMBytes(),
                        DiskInfos = GetDiskInfos()
                    };

                    var envelope = new Envelope { Type = "MonitorData", Payload = JsonSerializer.SerializeToElement(data) };
                    var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));
                    var len = BitConverter.GetBytes(bytes.Length);

                    lock (writeLock)
                    {
                        server.Write(len, 0, len.Length);
                        server.Write(bytes, 0, bytes.Length);
                        try { server.Flush(); } catch { }
                    }

                    await Task.Delay(getIntervalMs(), ct);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WriterLoop error: {ex.Message}");
            }
        }

        private static async Task ReaderLoopAsync(PipeStream server, object writeLock, Action<string, JsonElement> onCommand, CancellationToken ct)
        {
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
                        if (r == 0) return;
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
                                onCommand(type, p);
                            }
                        }
                    }
                    catch (JsonException) { }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Console.WriteLine($"ReaderLoop error: {ex.Message}");
            }
        }

        private static void HandleCommand(string cmd, JsonElement payload, PipeStream server, object writeLock, ref int intervalMs)
        {
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
                    SendEnvelope(server, writeLock, env);
                }
                else if (cmd == "TriggerGC")
                {
                    GC.Collect();
                    var env = new Envelope { Type = "CommandAck", Payload = JsonSerializer.SerializeToElement(new { Command = "TriggerGC", Status = "OK" }) };
                    SendEnvelope(server, writeLock, env);
                }
                else if (cmd == "SetIntervalSeconds")
                {
                    if (payload.TryGetProperty("Seconds", out var s) && s.TryGetInt32(out var seconds))
                    {
                        intervalMs = Math.Max(200, seconds * 1000);
                        var env = new Envelope { Type = "CommandAck", Payload = JsonSerializer.SerializeToElement(new { Command = "SetIntervalSeconds", Status = "OK", Seconds = seconds }) };
                        SendEnvelope(server, writeLock, env);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HandleCommand error: {ex.Message}");
            }
        }

        private static void SendEnvelope(PipeStream server, object writeLock, Envelope env)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(env));
                var len = BitConverter.GetBytes(bytes.Length);
                lock (writeLock)
                {
                    server.Write(len, 0, len.Length);
                    server.Write(bytes, 0, bytes.Length);
                    try { server.Flush(); } catch { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendEnvelope failed: {ex.Message}");
            }
        }

        private static float GetTotalMemoryInMBytes()
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

        private static DiskInfo[] GetDiskInfos()
        {
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady).Select(d => new DiskInfo
            {
                Name = d.Name,
                TotalGB = d.TotalSize / (1024.0 * 1024 * 1024),
                FreeGB = d.TotalFreeSpace / (1024.0 * 1024 * 1024)
            }).ToArray();
            return drives;
        }

        private class Envelope { public string Type { get; set; } public JsonElement Payload { get; set; } }
    }
}
