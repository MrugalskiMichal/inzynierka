using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ManagerServer.Controllers
{
    [ApiController]
    [Route("api/server-address")]
    public class ServerAddressController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ServerAddressController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var address = _config["ServerAddress"];
            return Ok(new { address });
        }
    }
}
