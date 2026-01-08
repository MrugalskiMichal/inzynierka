using ManagerServer.Models;
using MongoDB.Driver;
using MongoDB.Bson;

public class AlertRepository
{
    private readonly IMongoCollection<AlertRule> _rules;
    private readonly IMongoCollection<AlertHistory> _history;

    public AlertRepository(IConfiguration config)
    {
        var client = new MongoClient(config["Mongo:ConnectionString"]);
        var db = client.GetDatabase(config["Mongo:Database"]);

        _rules = db.GetCollection<AlertRule>("AlertRules");
        _history = db.GetCollection<AlertHistory>("AlertHistory");
    }

    public async Task<List<AlertRule>> GetActiveRulesAsync()
    {
        return await _rules.Find(r => r.IsActive).ToListAsync();
    }

    public async Task AddHistoryAsync(AlertHistory history)
    {
        await _history.InsertOneAsync(history);
    }

    public async Task UpdateRuleAsync(AlertRule rule)
    {
        await _rules.ReplaceOneAsync(r => r.Id == rule.Id, rule);
    }

    public async Task AddRuleAsync(AlertRule rule)
    {
        Console.WriteLine("Próba zapisu alertu:");
        Console.WriteLine($"AgentId: {rule.AgentId}");
        Console.WriteLine($"MetricType: {rule.MetricType}");
        Console.WriteLine($"MetricName: {rule.MetricName}");
        Console.WriteLine($"Threshold: {rule.Threshold}");
        Console.WriteLine($"Email: {rule.Email}");
        Console.WriteLine($"Operator: {rule.Operator}");
        Console.WriteLine($"DiskName: {rule.DiskName}");
        Console.WriteLine($"Id: {rule.Id}");

        try
        {
            await _rules.InsertOneAsync(rule);
            Console.WriteLine("✅ Zapisano alert w MongoDB.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Błąd zapisu alertu: {ex.Message}");
        }
    }

    public async Task DeleteRuleAsync(string id)
    {
        await _rules.DeleteOneAsync(r => r.Id == id);
    }
}
