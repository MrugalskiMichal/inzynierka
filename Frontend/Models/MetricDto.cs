namespace Frontend.Models
{
    class MetricDto
    {
        public string AgentId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public float? CpuUsagePercent { get; set; }
        public RamDto? Ram { get; set; }
        public Dictionary<string, DiskDto>? Disks { get; set; }
        public Dictionary<string, GpuDto>? Gpu { get; set; }
    }
}