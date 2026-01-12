using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ManagerServer.Models;
using ManagerServer.Models.Dto;

namespace ManagerServer.Controllers
{
    [ApiController]
    [Route("api/agents")]
    public class AgentsController : ControllerBase
    {
        private readonly IMongoCollection<Agent> _agents;

        public AgentsController()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("timeseriesdb");
            _agents = database.GetCollection<Agent>("agents");
        }

        // GET: api/agents
        [HttpGet]
        public async Task<ActionResult<List<AgentDto>>> GetAgents()
        {
            var agents = await _agents.Find(_ => true).ToListAsync();

            return agents.Select(a => new AgentDto
            {
                AgentId = a.AgentId,
                AuthToken = a.AuthToken
            }).ToList();
        }

        // POST: api/agents
        [HttpPost]
        public async Task<IActionResult> CreateAgent([FromBody] AgentDto dto)
        {
            var exists = await _agents.Find(a => a.AgentId == dto.AgentId).AnyAsync();
            if (exists)
                return Conflict("Agent with this ID already exists.");

            var agent = new Agent
            {
                AgentId = dto.AgentId,
                AuthToken = dto.AuthToken
            };

            await _agents.InsertOneAsync(agent);
            return Ok();
        }

        // PUT: api/agents
        [HttpPut]
        public async Task<IActionResult> UpdateAgent([FromBody] AgentDto dto)
        {
            var update = Builders<Agent>.Update
                .Set(a => a.AuthToken, dto.AuthToken);

            var result = await _agents.UpdateOneAsync(a => a.AgentId == dto.AgentId, update);

            if (result.MatchedCount == 0)
                return NotFound("Agent not found.");

            return Ok();
        }

        // DELETE: api/agents/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAgent(string id)
        {
            var result = await _agents.DeleteOneAsync(a => a.AgentId == id);

            if (result.DeletedCount == 0)
                return NotFound("Agent not found.");

            return Ok();
        }
    }
}
