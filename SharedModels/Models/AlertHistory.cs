using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class AlertHistory
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    // Powiązanie z regułą alertu
    [BsonRepresentation(BsonType.ObjectId)]
    public string AlertRuleId { get; set; } = string.Empty;


    // Kiedy alert został wywołany
    public DateTime Timestamp { get; set; }

    // Wartość metryki, która spowodowała alert
    public double TriggerValue { get; set; }

    // Dodatkowe info (np. nazwa dysku, CPU, RAM)
    public string? Details { get; set; }
}
