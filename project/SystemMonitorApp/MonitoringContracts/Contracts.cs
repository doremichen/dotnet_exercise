using System.Text.Json;

namespace MonitoringContracts
{
    public class ProcessInfo
    {
        public int Id { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public double WorkingSetMB { get; set; }
        // versioning / metadata
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public string Version { get; set; } = "1.0";
    }

    public class DiskInfo
    {
        public string Name { get; set; } = string.Empty;
        public double TotalGB { get; set; }
        public double FreeGB { get; set; }
    }

    public class MonitorData
    {
        public double CpuUsage { get; set; }
        public float AvailableMemoryMB { get; set; }
        public float TotalMemoryMB { get; set; }
        public DiskInfo[]? DiskInfos { get; set; }
    }

    public class Envelope { public string Type { get; set; } public JsonElement Payload { get; set; } }
}
