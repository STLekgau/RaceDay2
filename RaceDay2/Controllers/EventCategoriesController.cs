using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDay2.Data;
using RaceDay2.Extensions;

namespace RaceDay2.Controllers
{
    [ApiController]
    [Route("api/event-categories")]
    [Authorize]
    public class EventCategoriesController : ControllerBase
    {
        private readonly EventCategoryRepository _repository;
        private readonly EventRepository _events;

        public EventCategoriesController(
            EventCategoryRepository repository,
            EventRepository events)
        {
            _repository = repository;
            _events = events;
        }

        [AllowAnonymous]
        [HttpGet("event/{eventId:int}")]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            return Ok(await _repository.GetByEventAsync(eventId));
        }

        [Authorize(Roles = "ORGANISER")]
        [HttpPost]
        public async Task<IActionResult> Add(
            [FromQuery] int eventId, [FromQuery] int categoryId)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var ev = await _events.GetByIdAsync(eventId);
            if (ev == null)
                return NotFound("Event not found.");

            if (ev.OrganiserID != userId)
                return Forbid();

            try
            {
                int id = await _repository.AddAsync(eventId, categoryId);
                return Ok(new
                {
                    EventCategoryID = id,
                    Message = "Category linked to event."
                });
            }
            catch (SqlException)
            {
                return Conflict(
                    "Could not link category. Check the category ID; the link may already exist.");
            }
        }

        [Authorize(Roles = "ORGANISER")]
        [HttpDelete]
        public async Task<IActionResult> Remove(
            [FromQuery] int eventId, [FromQuery] int categoryId)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var ev = await _events.GetByIdAsync(eventId);
            if (ev == null)
                return NotFound("Event not found.");

            if (ev.OrganiserID != userId)
                return Forbid();

            try
            {
                return await _repository.RemoveAsync(eventId, categoryId)
                    ? NoContent()
                    : NotFound();
            }
            catch (SqlException)
            {
                return Conflict("Could not remove the event-category link.");
            }
        }
    }
}