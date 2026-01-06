using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent
{
    public class MetricsSnapshot
    {
        public string agentId { get; set; }
        public DateTime timestamp { get; set; }
        public float? cpuUsagePercent { get; set; }
        public RamData? ram { get; set; }
        public Dictionary<string, DiskData>? disks { get; set; }
        public Dictionary<string, GpuData>? gpu { get; set; }
    }
    public class RamData
    {
        public RamData(float usedRamMB, float totalRamMB)
        {
            this.usedRamMB = MathF.Round(usedRamMB, 2);
            this.totalRamMB = MathF.Round(totalRamMB, 2);
            ramUsageMB = MathF.Round((usedRamMB / totalRamMB) * 100, 2);
        }

        public float usedRamMB { get; set; }
        public float totalRamMB { get; set; }
        public float ramUsageMB { get; set; }
    }
    public class GpuData
    {
        public float? gpuCoreUsage {  get; set; }
        public float? gpuCoreTemperature { get; set; }
        public float? gpuMemoryUsage { get; set; }       
    }
    public class DiskData
    {
        public long usedDiskMB { get; set; }
        public long totalDiskMB { get; set; }
        public double usageDiskPercent { get; set; }
    }
}
