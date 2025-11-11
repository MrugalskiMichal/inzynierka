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
        public float cpuUsagePercent { get; set; }
        public float usedRamMB { get; set; }
        public float totalRamMB { get; set; }
        public Dictionary<string, DiskUsage> disks { get; set; }
    }

    public class DiskUsage
    {
        public long usedDiskMB { get; set; }
        public long totalDiskMB { get; set; }
        public double usageDiskPercent { get; set; }
    }
}
