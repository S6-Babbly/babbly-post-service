using Microsoft.AspNetCore.Mvc;
using babbly_post_service.Data;

namespace babbly_post_service.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        private readonly CassandraContext _context;

        public HealthController(CassandraContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "Healthy", service = "babbly-post-service" });
        }

        [HttpGet("database")]
        public IActionResult CheckDatabase()
        {
            try
            {
                // Check if Cassandra is accessible
                var session = _context.Session;
                var rows = session.Execute("SELECT release_version FROM system.local");
                var version = rows.First().GetValue<string>("release_version");
                
                return Ok(new { status = "Database connection healthy", version });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "Database connection error", message = ex.Message });
            }
        }
    }
}