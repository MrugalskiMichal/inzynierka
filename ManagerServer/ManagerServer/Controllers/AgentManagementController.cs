using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ManagerServer.Models;

namespace ManagerServer.Controllers
{
    [Route("api/agent-management")]
    [ApiController]
    public class AgentManagementController : ControllerBase
    {
        private readonly IMongoCollection<Agent> _agents;

        public AgentManagementController()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("timeseriesdb");
            _agents = database.GetCollection<Agent>("agents");
        }

        [HttpGet]
        public async Task<IActionResult> GetAgents()
        {
            var list = await _agents.Find(_ => true).ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> AddAgent([FromBody] Agent agent)
        {
            await _agents.InsertOneAsync(agent);
            return Ok(agent);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAgent([FromBody] Agent agent)
        {
            var filter = Builders<Agent>.Filter.Eq(a => a.AgentId, agent.AgentId);
            var update = Builders<Agent>.Update.Set(a => a.AuthToken, agent.AuthToken);

            var result = await _agents.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                return NotFound();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAgent(string id)
        {
            await _agents.DeleteOneAsync(a => a.AgentId == id);
            return Ok();
        }
    }
}
