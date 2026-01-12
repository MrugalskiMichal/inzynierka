using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ManagerServer.Models
{
    public class AlertHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string AlertRuleId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public double TriggerValue { get; set; }
        public string? Details { get; set; }
    }
}
