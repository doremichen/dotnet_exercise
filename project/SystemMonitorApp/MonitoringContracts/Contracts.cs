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
