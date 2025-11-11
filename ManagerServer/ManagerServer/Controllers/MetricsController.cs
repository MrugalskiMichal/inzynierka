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
        [HttpPost]
        public async Task<IActionResult> ReceiveMetrics([FromBody] MetricsSnapshot snapshot)
        {
            try
            {
                var client = new MongoClient("mongodb://localhost:27017");
                //to jest nazwa bazy danych jak coś
                var database = client.GetDatabase("timeseriesdb");

                //wybierz/stwórz kolekcję jeśli jej nie ma
                var collection = database.GetCollection<MetricsSnapshot>("metrics");

                //insert snapshot
                await collection.InsertOneAsync(snapshot);

                return Ok(new { message = "Metrics saved to MongoDB." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error saving metrics.", error = ex.Message });
            }

        }
    }
}
