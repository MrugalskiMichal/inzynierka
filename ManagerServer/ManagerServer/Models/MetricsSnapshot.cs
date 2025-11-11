using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ManagerServer.Models
{
    public class MetricsSnapshot
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("agentId")]
        public string AgentId { get; set; }
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; }
        [BsonElement("cpuUsagePercent")]
        public float CpuUsagePercent { get; set; }
        [BsonElement("usedRamMB")]
        public float UsedRamMB { get; set; }
        [BsonElement("totalRamMB")]
        public float TotalRamMB { get; set; }
        [BsonElement("disks")]
        public Dictionary<string, DiskUsage> Disks { get; set; }
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
