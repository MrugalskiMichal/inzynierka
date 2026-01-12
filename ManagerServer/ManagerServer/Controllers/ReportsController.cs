using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ManagerServer.Models;
using ManagerServer.Models.Dto;

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
    public async Task<IEnumerable<ReportRuleDto>> GetAll()
    {
        var list = await _rules.Find(_ => true).ToListAsync();

        return list.Select(r => new ReportRuleDto
        {
            Id = r.Id,
            Name = r.Name,
            AgentIds = r.AgentIds,
            Metrics = r.Metrics,
            IntervalHours = r.IntervalHours,
            Email = r.Email
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReportRuleDto dto)
    {
        var rule = new ReportRule
        {
            Name = dto.Name,
            AgentIds = dto.AgentIds,
            Metrics = dto.Metrics,
            IntervalHours = dto.IntervalHours,
            Email = dto.Email,
            LastSent = null
        };

        await _rules.InsertOneAsync(rule);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _rules.DeleteOneAsync(r => r.Id == id);
        return Ok();
    }
}
