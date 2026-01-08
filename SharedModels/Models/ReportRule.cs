using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SharedModels.Models
{
    [BsonIgnoreExtraElements]
    public class ReportRule
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<string> AgentIds { get; set; } = new();
        public List<string> Metrics { get; set; } = new();

        [BsonElement("intervalHours")]
        [Range(0, int.MaxValue, ErrorMessage = "Interval must be >= 0")]
        public int IntervalHours { get; set; }
        public DateTime? LastSent { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
