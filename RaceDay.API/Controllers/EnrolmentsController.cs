using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Enrolments
        [HttpGet]
        public async Task<IActionResult> GetEnrolments()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return Unauthorized("Please log in first.");

            if (role == "Participant")
            {
                var myEnrolments = await _context.Enrolments
                    .Where(e => e.ParticipantID == userId.Value)
                    .ToListAsync();

                return Ok(myEnrolments);
            }

            if (role == "Organiser")
            {
                var eventIds = await _context.Events
                    .Where(e => e.OrganiserID == userId.Value)
                    .Select(e => e.EventID)
                    .ToListAsync();

                var enrolments = await _context.Enrolments
                    .Where(e => eventIds.Contains(e.EventID))
                    .ToListAsync();

                return Ok(enrolments);
            }

            return StatusCode(403, "Access denied.");
        }

        // GET: api/Enrolments/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEnrolment(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return Unauthorized("Please log in first.");

            var enrolment = await _context.Enrolments.FindAsync(id);

            if (enrolment == null)
                return NotFound();

            if (role == "Participant" &&
                enrolment.ParticipantID == userId.Value)
                return Ok(enrolment);

            if (role == "Organiser")
            {
                bool ownsEvent = await _context.Events.AnyAsync(
                    e => e.EventID == enrolment.EventID &&
                         e.OrganiserID == userId.Value);

                if (ownsEvent)
                    return Ok(enrolment);
            }

            return StatusCode(403, "Access denied.");
        }

        // POST: api/Enrolments
        [HttpPost]
        public async Task<IActionResult> CreateEnrolment(
            Enrolment enrolment)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return Unauthorized("Please log in first.");

            if (role != "Participant")
                return StatusCode(403,
                    "Only Participants can enrol in events.");

            enrolment.ParticipantID = userId.Value;

            bool eventExists = await _context.Events
                .AnyAsync(e => e.EventID == enrolment.EventID);

            if (!eventExists)
                return BadRequest("The selected event does not exist.");

            bool validCategory = await _context.Categories
                .AnyAsync(c =>
                    c.CategoryID == enrolment.CategoryID &&
                    c.EventID == enrolment.EventID);

            if (!validCategory)
                return BadRequest(
                    "The selected category does not belong to this event.");

            bool alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e =>
                    e.ParticipantID == userId.Value &&
                    e.EventID == enrolment.EventID);

            if (alreadyEnrolled)
                return Conflict(
                    "You are already enrolled in this event.");

            enrolment.EnrolmentID = 0;
            enrolment.EnrolmentDate = DateTime.Now;

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEnrolment),
                new { id = enrolment.EnrolmentID },
                enrolment);
        }

        // PUT: api/Enrolments/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEnrolment(
            int id, Enrolment enrolment)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return Unauthorized("Please log in first.");

            if (role != "Participant")
                return StatusCode(403,
                    "Only Participants can update their enrolments.");

            if (id != enrolment.EnrolmentID)
                return BadRequest("Enrolment ID does not match.");

            var existingEnrolment =
                await _context.Enrolments.FindAsync(id);

            if (existingEnrolment == null)
                return NotFound();

            if (existingEnrolment.ParticipantID != userId.Value)
                return StatusCode(403,
                    "You can only update your own enrolment.");

            bool validCategory = await _context.Categories
                .AnyAsync(c =>
                    c.CategoryID == enrolment.CategoryID &&
                    c.EventID == existingEnrolment.EventID);

            if (!validCategory)
                return BadRequest(
                    "The selected category does not belong to this event.");

            existingEnrolment.CategoryID = enrolment.CategoryID;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Enrolments/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrolment(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return Unauthorized("Please log in first.");

            if (role != "Participant")
                return StatusCode(403,
                    "Only Participants can cancel enrolments.");

            var enrolment = await _context.Enrolments.FindAsync(id);

            if (enrolment == null)
                return NotFound();

            if (enrolment.ParticipantID != userId.Value)
                return StatusCode(403,
                    "You can only cancel your own enrolment.");

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Enrolments/organiser-registrations
        [HttpGet("organiser-registrations")]
        public async Task<IActionResult> GetOrganiserRegistrations()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return Unauthorized("Please log in first.");

            if (role != "Organiser")
                return StatusCode(403,
                    "Only organisers can view these registrations.");

            var registrations = await (
                from enrolment in _context.Enrolments
                join raceEvent in _context.Events
                    on enrolment.EventID equals raceEvent.EventID
                join participant in _context.Users
                    on enrolment.ParticipantID equals participant.UserID
                join category in _context.Categories
                    on enrolment.CategoryID equals category.CategoryID
                where raceEvent.OrganiserID == userId.Value
                orderby enrolment.EnrolmentDate descending
                select new
                {
                    enrolment.EnrolmentID,
                    enrolment.ParticipantID,
                    enrolment.EventID,
                    enrolment.CategoryID,
                    enrolment.EnrolmentDate,
                    raceEvent.EventName,
                    ParticipantName =
                        participant.FirstName + " " + participant.LastName,
                    CategoryName = category.CategoryName
                }
            ).AsNoTracking().ToListAsync();

            return Ok(registrations);
        }
    }
}
