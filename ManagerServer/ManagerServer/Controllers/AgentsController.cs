using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ManagerServer.Models;

namespace ManagerServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentsController : ControllerBase
    {
        private readonly IMongoCollection<Agent> _agents;

        public AgentsController()
        {
            var client = new MongoClient("mongodb://localhost:27017"); 
            var database = client.GetDatabase("timeseriesdb");
            _agents = database.GetCollection<Agent>("agents");
        }

        [HttpGet]
        public async Task<IActionResult> GetAgentsToList()
        {
            var agentsList = await _agents.Find(Builders<Agent>.Filter.Empty).ToListAsync();
            return Ok(agentsList);
        }


        [HttpPost]
        public async Task<IActionResult> AddAgent([FromBody] Agent agent)
        {
            var exists = await _agents.Find(a => a.AgentId == agent.AgentId).FirstOrDefaultAsync();
            if (exists != null)
                return Conflict(new { message = "Agent with this Id already exists." });

            await _agents.InsertOneAsync(agent);
            return Ok(agent);
        }

        [HttpPut("{agentId}")]
        public async Task<IActionResult> UpdateToken(string agentId, [FromBody]string newToken)
        {
            var builder = Builders<Agent>.Update.Set(u => u.AuthToken, newToken);
            var update = await _agents.UpdateOneAsync(a => a.AgentId == agentId, builder);

            if(update.MatchedCount == 0)
                return NotFound(new { message = $"Agent {agentId} not found." });

            return Ok(new { message = $"Token updated for agent {agentId}." });
        }

        [HttpDelete("{agentId}")]
        public async Task<IActionResult> DeleteAgent(string agentId)
        {
            var delete = await _agents.DeleteOneAsync(a => a.AgentId == agentId);

            if (delete.DeletedCount == 0)
                return NotFound(new { message = $"Agent {agentId} not found." });

            return Ok(new { message = $"Agent {agentId} deleted." });
        }

    }
}
