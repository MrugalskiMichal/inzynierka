using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ManagerServer.Models
{
    public class MetricsSnapshot
    {
        //Bsona używam żeby format był zgodny z formatem jsonowym który od małej zaczyna, i C# który z dużej

        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("agentId")]
        public string AgentId { get; set; }
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; }
        [BsonElement("cpuUsagePercent")]
        public float? CpuUsagePercent { get; set; }

        [BsonElement("ram")]
        public RamData? Ram { get; set; }

        [BsonElement("disks")]
        public Dictionary<string, DiskUsage>? Disks { get; set; }
        [BsonElement("gpu")]
        public Dictionary<string, GpuData>? Gpu { get; set; }

    }

    public class RamData
    {
        [BsonElement("usedRamMB")]
        public float UsedRamMB { get; set; }
        [BsonElement("totalRamMB")]
        public float TotalRamMB { get; set; }
        [BsonElement("ramUsageMB")]
        public float RamUsageMB { get; set; }
    }
    public class GpuData
    {
        [BsonElement("gpuCoreUsage")]
        public float? GpuCoreUsage { get; set; }
        [BsonElement("gpuCoreTemperature")]
        public float? GpuCoreTemperature { get; set; }
        [BsonElement("gpuMemoryUsage")]
        public float? GpuMemoryUsage { get; set; }
    }
    public class DiskUsage
    {
        [BsonElement("usedDiskMB")]
        public long UsedDiskMB { get; set; }
        [BsonElement("totalDiskMB")]
        public long TotalDiskMB { get; set; }
        [BsonElement("usageDiskPercent")]
        public double UsageDiskPercent { get; set; }
    }
}
