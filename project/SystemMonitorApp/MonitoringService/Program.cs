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
        // are unavailable or if permissions are insufficient.
        private static PerformanceCounter? _cpuCounter;
        private static PerformanceCounter? _memoryCounter;


        private static readonly SemaphoreSlim ServerWriteSemaphore = new SemaphoreSlim(1, 1);

        static void Main(string[] args)
        {
            Debug.WriteLine("[INIT] MonitoringService is initializing counters...");
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
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _cpuCounter.NextValue();
                Debug.WriteLine("[SUCCESS] CPU performance counter initialized successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CRITICAL] CPU counter initialization failed (possibly insufficient permissions): {ex.Message}");
            }

            try
            {
                _memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
                _memoryCounter.NextValue();
                Debug.WriteLine("[SUCCESS] Memory performance counter initialized successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CRITICAL] Memory counter initialization failed: {ex.Message}");
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

                    try { if (_cpuCounter != null) cpuVal = _cpuCounter.NextValue(); }
                    catch (Exception ex) { Debug.WriteLine($"[DATA ERROR] Failed to read CPU: {ex.Message}"); }

                    try { if (_memoryCounter != null) memVal = _memoryCounter.NextValue(); }
                    catch (Exception ex) { Debug.WriteLine($"[DATA ERROR] Failed to read Memory: {ex.Message}"); }


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

                    await ServerWriteSemaphore.WaitAsync(ct);
                    try
                    {
                        if (server.IsConnected)
                        {
                            await server.WriteAsync(len, 0, len.Length, ct);
                            await server.WriteAsync(bytes, 0, bytes.Length, ct);
                            await server.FlushAsync(ct);
                            Debug.WriteLine($"[SEND] MonitorData sent, length: {bytes.Length} bytes. CPU: {cpuVal:F1}%, RAM Free: {memVal}MB");

                        }
                    }
                    finally
                    {
                        ServerWriteSemaphore.Release();
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
                                Console.WriteLine($"[RECEIVE] Received client command: {type}");
                                onCommand(type, p);
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        Debug.WriteLine($"[JSON ERROR] Failed to parse client message: {ex.Message}");
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
                ServerWriteSemaphore.Wait();
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
                    ServerWriteSemaphore.Release();
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
