using ManagerServer.Models;
using MongoDB.Driver;

public class MetricsRepository
{
    private readonly IMongoCollection<MetricsSnapshot> _metrics;

    public MetricsRepository(IConfiguration config)
    {
        var client = new MongoClient(config["Mongo:ConnectionString"]);
        var db = client.GetDatabase(config["Mongo:Database"]);

        _metrics = db.GetCollection<MetricsSnapshot>("metrics");
    }

    public async Task<List<MetricsSnapshot>> GetMetricsForLastHour(string agentId)
    {
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);

        return await _metrics
            .Find(m => m.AgentId == agentId && m.Timestamp >= oneHourAgo)
            .ToListAsync();
    }
}
