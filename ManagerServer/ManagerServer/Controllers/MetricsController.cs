using ManagerServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ManagerServer.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class MetricsController : ControllerBase
    {
        private string FilePath = "servermetrics.json";

        [HttpPost]
        public IActionResult ReceiveMetrics([FromBody] MetricsSnapshot snapshot)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                };

                string json = JsonSerializer.Serialize(snapshot, options);
                System.IO.File.WriteAllText(FilePath, json);

                return Ok(new { message = "Metrics saved to file." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error saving metrics.", error = ex.Message });
            }

        }

        /*
        public IActionResult Index()
        {
            return View();
        }*/
    }
}
