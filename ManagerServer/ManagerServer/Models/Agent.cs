using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ManagerServer.Models
{
    public class Agent
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string AgentId { get; set; }
        [BsonElement("authToken")]
        public string AuthToken { get; set; }
    }
}
