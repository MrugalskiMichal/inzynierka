using ManagerServer.Models;
using MongoDB.Driver;
using MongoDB.Bson;

public class AlertRepository
{
    private readonly IMongoCollection<AlertRule> _rules;
    private readonly IMongoCollection<AlertHistory> _history;

    public AlertRepository(IMongoDatabase db)
    {
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
        try
        {
            await _rules.InsertOneAsync(rule);
            Console.WriteLine("Saved in MongoDB");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Alert save error: {ex.Message}");
        }
    }

    public async Task DeleteRuleAsync(string id)
    {
        await _rules.DeleteOneAsync(r => r.Id == id);
    }
}
