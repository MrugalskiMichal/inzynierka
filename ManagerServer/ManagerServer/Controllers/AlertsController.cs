using Microsoft.AspNetCore.Mvc;
using ManagerServer.Models;

[ApiController]
[Route("api/[controller]")]
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
        return Ok(rules);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AlertRule rule)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .Select(e => new
                {
                    Field = e.Key,
                    Messages = e.Value.Errors.Select(err => err.ErrorMessage).ToList()
                });

            return BadRequest(new { Message = "Model validation failed", Errors = errors });
        }

        rule.IsActive = true;
        await _alertRepo.AddRuleAsync(rule);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        Console.WriteLine($"🔥 DELETE odebrany: {id}");
        await _alertRepo.DeleteRuleAsync(id);
        return Ok();
    }
}
