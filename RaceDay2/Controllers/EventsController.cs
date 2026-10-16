using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay2.Data;
using RaceDay2.DTOs;
using RaceDay2.Extensions;
using RaceDay2.Models;

namespace RaceDay2.Controllers
{
    [ApiController]
    [Route("api/events")]
    [Authorize]
    public class EventsController : ControllerBase
    {
        private readonly EventRepository _repository;

        public EventsController(EventRepository repository)
        {
            _repository = repository;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _repository.GetAllAsync());
        }

        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [Authorize(Roles = "ORGANISER")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateEventRequest request)
        {
            var organizerId = User.GetUserId();
            if (organizerId == null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.EventName) ||
                string.IsNullOrWhiteSpace(request.Location))
                return BadRequest("Event name and location are required.");

            if (request.Distance <= 0)
                return BadRequest("Distance must be greater than zero.");

            if (!new[] { "RUN", "WALK", "CYCLE" }
                .Contains(request.EventType.ToUpperInvariant()))
                return BadRequest("EventType must be RUN, WALK or CYCLE.");

            var item = new Event
            {
                OrganiserID = organizerId.Value,
                EventName = request.EventName,
                Description = request.Description,
                EventDate = request.EventDate,
                Location = request.Location,
                Distance = request.Distance,
                EventType = request.EventType.ToUpperInvariant()
            };

            try
            {
                int id = await _repository.CreateAsync(item);
                return CreatedAtAction(
                    nameof(GetById), new { id },
                    new { EventID = id, Message = "Event created." });
            }
            catch (Microsoft.Data.SqlClient.SqlException)
            {
                return BadRequest(
                    "Could not create event. Check the details and database constraints.");
            }
        }

        [Authorize(Roles = "ORGANISER")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id, CreateEventRequest request)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            // Only the organizer who owns the event may change it.
            if (existing.OrganiserID != userId)
                return Forbid();

            if (string.IsNullOrWhiteSpace(request.EventName) ||
                string.IsNullOrWhiteSpace(request.Location) ||
                request.Distance <= 0)
                return BadRequest("Provide valid event details.");

            if (!new[] { "RUN", "WALK", "CYCLE" }
                .Contains(request.EventType.ToUpperInvariant()))
                return BadRequest("EventType must be RUN, WALK or CYCLE.");

            var item = new Event
            {
                EventID = id,
                EventName = request.EventName,
                Description = request.Description,
                EventDate = request.EventDate,
                Location = request.Location,
                Distance = request.Distance,
                EventType = request.EventType.ToUpperInvariant()
            };

            try
            {
                return await _repository.UpdateAsync(item)
                    ? NoContent()
                    : NotFound();
            }
            catch (Microsoft.Data.SqlClient.SqlException)
            {
                return BadRequest("Could not update event.");
            }
        }

        [Authorize(Roles = "ORGANISER")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            if (existing.OrganiserID != userId)
                return Forbid();

            try
            {
                return await _repository.DeleteAsync(id)
                    ? NoContent()
                    : NotFound();
            }
            catch (Microsoft.Data.SqlClient.SqlException)
            {
                return Conflict(
                    "The event may have related enrollments or categories.");
            }
        }
    }
}