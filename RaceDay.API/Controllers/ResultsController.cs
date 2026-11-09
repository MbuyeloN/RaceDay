using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int? UserId =>
            HttpContext.Session.GetInt32("UserID");

        private string? Role =>
            HttpContext.Session.GetString("Role");

        private async Task<bool> CanViewResult(Result result)
        {
            if (UserId == null)
                return false;

            var enrolment = await _context.Enrolments
                .FindAsync(result.EnrolmentID);

            if (enrolment == null)
                return false;

            if (Role == "Participant")
                return enrolment.ParticipantID == UserId.Value;

            if (Role == "Organiser")
                return await _context.Events.AnyAsync(e =>
                    e.EventID == enrolment.EventID &&
                    e.OrganiserID == UserId.Value);

            return false;
        }

        // GET: api/Results
        [HttpGet]
        public async Task<IActionResult> GetResults()
        {
            if (UserId == null)
                return Unauthorized("Please log in first.");

            if (Role == "Participant")
            {
                var enrolmentIds = _context.Enrolments
                    .Where(e => e.ParticipantID == UserId.Value)
                    .Select(e => e.EnrolmentID);

                var results = await _context.Results
                    .Where(r => enrolmentIds.Contains(r.EnrolmentID))
                    .ToListAsync();

                return Ok(results);
            }

            if (Role == "Organiser")
            {
                var eventIds = _context.Events
                    .Where(e => e.OrganiserID == UserId.Value)
                    .Select(e => e.EventID);

                var enrolmentIds = _context.Enrolments
                    .Where(e => eventIds.Contains(e.EventID))
                    .Select(e => e.EnrolmentID);

                var results = await _context.Results
                    .Where(r => enrolmentIds.Contains(r.EnrolmentID))
                    .ToListAsync();

                return Ok(results);
            }

            return StatusCode(403, "Access denied.");
        }

        // GET: api/Results/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetResult(int id)
        {
            if (UserId == null)
                return Unauthorized("Please log in first.");

            var result = await _context.Results.FindAsync(id);

            if (result == null)
                return NotFound();

            if (!await CanViewResult(result))
                return StatusCode(403, "Access denied.");

            return Ok(result);
        }

        // POST: api/Results
        [HttpPost]
        public async Task<IActionResult> CreateResult(Result result)
        {
            if (UserId == null)
                return Unauthorized("Please log in first.");

            if (Role != "Organiser")
                return StatusCode(403,
                    "Only organisers can add results.");

            if (result.FinishingPosition <= 0 ||
                result.FinishTime <= TimeSpan.Zero)
                return BadRequest("Invalid result details.");

            var enrolment = await _context.Enrolments
                .FindAsync(result.EnrolmentID);

            if (enrolment == null)
                return BadRequest("Enrolment does not exist.");

            bool ownsEvent = await _context.Events.AnyAsync(e =>
                e.EventID == enrolment.EventID &&
                e.OrganiserID == UserId.Value);

            if (!ownsEvent)
                return StatusCode(403, "Access denied.");

            bool exists = await _context.Results.AnyAsync(r =>
                r.EnrolmentID == result.EnrolmentID);

            if (exists)
                return Conflict(
                    "A result already exists for this enrolment.");

            result.ResultID = 0;

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetResult),
                new { id = result.ResultID },
                result);
        }

        // PUT: api/Results/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResult(
            int id, Result result)
        {
            if (UserId == null)
                return Unauthorized("Please log in first.");

            if (Role != "Organiser")
                return StatusCode(403,
                    "Only organisers can update results.");

            if (id != result.ResultID)
                return BadRequest("Result ID does not match.");

            if (result.FinishingPosition <= 0 ||
                result.FinishTime <= TimeSpan.Zero)
                return BadRequest("Invalid result details.");

            var existing = await _context.Results.FindAsync(id);

            if (existing == null)
                return NotFound();

            if (!await CanViewResult(existing))
                return StatusCode(403, "Access denied.");

            if (result.EnrolmentID != existing.EnrolmentID)
                return BadRequest(
                    "The enrolment cannot be changed.");

            existing.FinishTime = result.FinishTime;
            existing.FinishingPosition = result.FinishingPosition;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Results/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            if (UserId == null)
                return Unauthorized("Please log in first.");

            if (Role != "Organiser")
                return StatusCode(403,
                    "Only organisers can delete results.");

            var result = await _context.Results.FindAsync(id);

            if (result == null)
                return NotFound();

            if (!await CanViewResult(result))
                return StatusCode(403, "Access denied.");

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
