using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ManagerServer.Models
{
    public class AlertRule
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string AgentId { get; set; } = string.Empty;
        public string MetricType { get; set; } = string.Empty;
        public string MetricName { get; set; } = string.Empty;
        public string? DiskName { get; set; }
        public string Operator { get; set; } = string.Empty;
        public double Threshold { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime? LastTriggered { get; set; }
    }
}
