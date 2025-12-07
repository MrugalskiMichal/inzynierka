using ManagerServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Text.Json;

namespace ManagerServer.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class MetricsController : ControllerBase
    {
        private readonly IMongoCollection<Agent> _agents;
        private readonly IMongoCollection<MetricsSnapshot> _metrics;

        public MetricsController()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("timeseriesdb");

            _agents = database.GetCollection<Agent>("agents");
            _metrics = database.GetCollection<MetricsSnapshot>("metrics");
        }


        // W tym kontrolerze są dwa gety,
        // jeden na całą listę agentów po agentID
        // drugi na szczegóły pojedynczego agenta od jakiejś godziny do jakiejś godziny

        // Jeden kontroler Post który otrzymuje metryki

        // zmienne client i database będą z czasem przeniesione do konstruktora i otrzymywane z głównego pliku program.cs

        [HttpGet("agents")]
        public async Task<IActionResult> GetAllAgentIds()
        {
            try
            {
                // zapisuje w liście, bierze tylko agentId
                var agentIdList = await _metrics
                    .Distinct<string>("AgentId", FilterDefinition<MetricsSnapshot>.Empty)
                    .ToListAsync();

                return Ok(agentIdList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving agent list.", error = ex.Message });
            }
        }

        [HttpGet("agents/{agentId}")]
        public async Task<IActionResult> GetAgentAllMetrics(string agentId, [FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            // FromQuery służy do dodatkowych parametrów w adresie URL
            // agentId tego nie ma bo jest kluczem
            // w adresie będzie to widoczne jako coś w stylu:
            // /api/metrics/agents/001?from=2025-12-05T18:00:00&to=2025-12-05T23:30:00

            try
            {

                // eq = m zmienna jest równa jakaś wartość 
                // gte = większe od
                // lte = mniejsze od
                var filter = Builders<MetricsSnapshot>.Filter.And(
                    Builders<MetricsSnapshot>.Filter.Eq(m => m.AgentId, agentId),
                    Builders<MetricsSnapshot>.Filter.Gte(m => m.Timestamp, from),
                    Builders<MetricsSnapshot>.Filter.Lte(m => m.Timestamp, to)
                );

                var agentMetricDetails = await _metrics.Find(filter).SortBy(m => m.Timestamp).ToListAsync();

                return Ok(agentMetricDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error retrieving metrics from agent: {agentId}", error = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> ReceiveMetrics([FromBody] MetricsSnapshot snapshot)
        {
            try
            {
                string? authToken = Request.Headers["X-Auth-Token"];

                if (string.IsNullOrEmpty(authToken))
                {
                    return Unauthorized(new { message = "No authorization token." });
                }

                var agent = await _agents.Find(a => a.AgentId == snapshot.AgentId).FirstOrDefaultAsync(); 

                if (agent == null || agent.AuthToken != authToken)
                {
                    return Unauthorized(new { message = "Invalid token or agent ID." });
                }

                await _metrics.InsertOneAsync(snapshot);

                return Ok(new { message = "Metrics saved to MongoDB." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error saving metrics.", error = ex.Message });
            }

        }
    }
}
