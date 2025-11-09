namespace ManagerServer.Models
{
    public class MetricsSnapshot
    {
        public string AgentId { get; set; }
        public DateTime Timestamp { get; set; }
        public float CpuUsagePercent { get; set; }
        public float UsedRamMB { get; set; }
        public float TotalRamMB { get; set; }
        public Dictionary<string, DiskUsage> Disks { get; set; }
    }

    public class DiskUsage
    {
        public long UsedDiskMB { get; set; }
        public long TotalDiskMB { get; set; }
        public double UsageDiskPercent { get; set; }
    }
}
