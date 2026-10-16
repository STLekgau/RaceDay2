
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDay2.Data;
using RaceDay2.DTOs;
using RaceDay2.Models;

namespace RaceDay2.Controllers
{
    [ApiController]
    [Route("api/results")]
    [Authorize]
    public class ResultsController : ControllerBase
    {
        private readonly ResultRepository _repository;

        public ResultsController(ResultRepository repository)
        {
            _repository = repository;
        }

        [AllowAnonymous]
        [HttpGet("enrollment/{enrollmentId:int}")]
        public async Task<IActionResult> GetByEnrollment(int enrollmentId)
        {
            var result = await _repository.GetByEnrollmentAsync(enrollmentId);
            return result == null ? NotFound() : Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("event/{eventId:int}")]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            return Ok(await _repository.GetByEventAsync(eventId));
        }

        // TODO: also check that the enrollment belongs to one of this organiser's events.
        [Authorize(Roles = "ORGANISER")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateResultRequest request)
        {
            if (request.EnrollmentID <= 0 ||
                request.FinishTime < TimeSpan.Zero ||
                request.FinishingPosition <= 0)
                return BadRequest("Provide a valid enrollment, finish time and finishing position.");

            var item = new Result
            {
                EnrollmentID = request.EnrollmentID,
                FinishTime = request.FinishTime,
                FinishingPosition = request.FinishingPosition
            };

            try
            {
                int id = await _repository.CreateAsync(item);
                return Ok(new { ResultID = id, Message = "Result created." });
            }
            catch (SqlException)
            {
                return Conflict(
                    "Could not create result. Check the enrollment ID and whether it already has a result.");
            }
        }

        // TODO: same ownership check as Create.
        [Authorize(Roles = "ORGANISER")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id, CreateResultRequest request)
        {
            if (request.FinishTime < TimeSpan.Zero ||
                request.FinishingPosition <= 0)
                return BadRequest("Provide a valid finish time and position.");

            var item = new Result
            {
                ResultID = id,
                FinishTime = request.FinishTime,
                FinishingPosition = request.FinishingPosition
            };

            try
            {
                return await _repository.UpdateAsync(item)
                    ? NoContent()
                    : NotFound();
            }
            catch (SqlException)
            {
                return Conflict("Could not update result.");
            }
        }
    }
}
