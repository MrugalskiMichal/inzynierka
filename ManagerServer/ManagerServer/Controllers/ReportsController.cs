using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SharedModels.Models;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IMongoCollection<ReportRule> _rules;

    public ReportsController(IMongoDatabase db)
    {
        _rules = db.GetCollection<ReportRule>("ReportRules");
    }

    [HttpGet]
    public async Task<IEnumerable<ReportRule>> GetAll()
        => await _rules.Find(_ => true).ToListAsync();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReportRule rule)
    {
        rule.LastSent = null;
        await _rules.InsertOneAsync(rule);
        return Ok(rule);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _rules.DeleteOneAsync(r => r.Id == id);
        return Ok();
    }
}
