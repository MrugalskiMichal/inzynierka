using Microsoft.AspNetCore.Mvc;
using ManagerServer.Models;
using ManagerServer.Models.Dto;

[ApiController]
[Route("api/alerts")]
public class AlertsController : ControllerBase
{
    private readonly AlertRepository _alertRepo;

    public AlertsController(AlertRepository alertRepo)
    {
        _alertRepo = alertRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rules = await _alertRepo.GetActiveRulesAsync();

        var dto = rules.Select(r => new AlertRuleDto
        {
            Id = r.Id,
            AgentId = r.AgentId,
            MetricType = r.MetricType,
            MetricName = r.MetricName,
            Operator = r.Operator,
            Threshold = r.Threshold,
            Email = r.Email,
            DiskName = r.DiskName
        });

        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AlertRuleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var rule = new AlertRule
        {
            AgentId = dto.AgentId,
            MetricType = dto.MetricType,
            MetricName = dto.MetricName,
            Operator = dto.Operator,
            Threshold = dto.Threshold,
            Email = dto.Email,
            DiskName = dto.DiskName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _alertRepo.AddRuleAsync(rule);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _alertRepo.DeleteRuleAsync(id);
        return Ok();
    }
}
