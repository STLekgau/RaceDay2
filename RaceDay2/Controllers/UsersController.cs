
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay2.Data;
using RaceDay2.Extensions;

namespace RaceDay2.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserRepository _repository;

        public UsersController(UserRepository repository)
        {
            _repository = repository;
        }

        // Users can view their own profile; organisers can view any profile.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            if (userId != id && !User.IsInRole("ORGANISER"))
                return Forbid();

            var user = await _repository.GetByIdAsync(id);

            if (user == null)
                return NotFound();

            // Never expose password hashes in API responses.
            return Ok(new
            {
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role,
                user.DateRegistered
            });
        }
    }
}