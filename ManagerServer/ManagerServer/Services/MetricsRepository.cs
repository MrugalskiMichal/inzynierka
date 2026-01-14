using ManagerServer.Models;
using MongoDB.Driver;

public class MetricsRepository
{
    private readonly IMongoCollection<MetricsSnapshot> _metrics;

    public MetricsRepository(IMongoDatabase db)
    {
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
