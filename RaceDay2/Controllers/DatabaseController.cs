using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay2.Data;

namespace RaceDay2.Controllers
{
    [ApiController]
    [Route("api/database")]
    [Authorize(Roles = "ORGANISER")]
    public class DatabaseController : ControllerBase
    {
        private readonly DatabaseConnectionTest _test;

        public DatabaseController(DatabaseConnectionTest test)
        {
            _test = test;
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                await _test.TestConnectionAsync();

                return Ok(new
                {
                    Message = "Connected to RaceDayDB successfully."
                });
            }
            catch (Exception)
            {
                // Details are deliberately not returned to the caller.
                return StatusCode(500, new
                {
                    Message = "Database connection failed."
                });
            }
        }
    }
}