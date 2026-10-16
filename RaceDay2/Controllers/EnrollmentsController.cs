
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDay2.Data;
using RaceDay2.DTOs;
using RaceDay2.Extensions;
using RaceDay2.Models;

namespace RaceDay2.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    [Authorize]
    public class EnrollmentsController : ControllerBase
    {
        private readonly EnrollmentRepository _repository;
        private readonly EventRepository _events;
        private readonly EventCategoryRepository _eventCategories;

        public EnrollmentsController(
            EnrollmentRepository repository,
            EventRepository events,
            EventCategoryRepository eventCategories)
        {
            _repository = repository;
            _events = events;
            _eventCategories = eventCategories;
        }

        // A participant can see their own enrollments; organisers can see anyone's.
        [HttpGet("participant/{participantId:int}")]
        public async Task<IActionResult> GetByParticipant(int participantId)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            if (userId != participantId && !User.IsInRole("ORGANISER"))
                return Forbid();

            return Ok(await _repository.GetByParticipantAsync(participantId));
        }

        // Only the organiser who owns the event can see who is enrolled.
        [Authorize(Roles = "ORGANISER")]
        [HttpGet("event/{eventId:int}")]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var ev = await _events.GetByIdAsync(eventId);
            if (ev == null)
                return NotFound("Event not found.");

            if (ev.OrganiserID != userId)
                return Forbid();

            return Ok(await _repository.GetByEventAsync(eventId));
        }

        [Authorize(Roles = "PARTICIPANT")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateEnrollmentRequest request)
        {
            var participantId = User.GetUserId();
            if (participantId == null)
                return Unauthorized();

            if (request.EventID <= 0 || request.CategoryID <= 0)
                return BadRequest("Valid event and category IDs are required.");

            // Rule 1: the event must exist and must not have happened yet.
            var ev = await _events.GetByIdAsync(request.EventID);
            if (ev == null)
                return NotFound("Event not found.");

            if (ev.EventDate < DateTime.Now)
                return BadRequest("Enrollment is closed because this event has already taken place.");

            // Rule 2: the category must be one of the categories offered for this event.
            var offered = await _eventCategories.GetByEventAsync(request.EventID);
            if (!offered.Any(c => c.CategoryID == request.CategoryID))
                return BadRequest("This category is not offered for the selected event.");

            // Rule 3: a participant can only enroll once per event.
            var existing = await _repository.GetByParticipantAsync(participantId.Value);
            if (existing.Any(e => e.EventID == request.EventID))
                return Conflict("You are already enrolled in this event.");

            var item = new Enrollment
            {
                ParticipantID = participantId.Value,
                EventID = request.EventID,
                CategoryID = request.CategoryID
            };

            try
            {
                int id = await _repository.CreateAsync(item);
                return Ok(new
                {
                    EnrollmentID = id,
                    Message = "Enrollment created."
                });
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // Unique constraint hit, e.g. two requests arriving at the same time.
                return Conflict("You are already enrolled in this event.");
            }
            catch (SqlException)
            {
                return Conflict(
                    "Could not enroll. Check the IDs and database constraints.");
            }
        }

        // The participant can cancel their own enrollment; the event's organiser can remove it too.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var enrollment = await _repository.GetByIdAsync(id);
            if (enrollment == null)
                return NotFound();

            bool isOwner = enrollment.ParticipantID == userId;
            bool isEventOrganizer = false;

            if (!isOwner && User.IsInRole("ORGANISER"))
            {
                var ev = await _events.GetByIdAsync(enrollment.EventID);
                isEventOrganizer = ev?.OrganiserID == userId;
            }

            if (!isOwner && !isEventOrganizer)
                return Forbid();

            try
            {
                return await _repository.DeleteAsync(id)
                    ? NoContent()
                    : NotFound();
            }
            catch (SqlException)
            {
                return Conflict("Could not delete enrollment because it may have a result.");
            }
        }
    }
}
