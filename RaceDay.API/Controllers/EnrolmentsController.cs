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
        public async Task<ActionResult<IEnumerable<Enrolment>>> GetEnrolments()
        {
            return await _context.Enrolments.ToListAsync();
        }

        // GET: api/Enrolments/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Enrolment>> GetEnrolment(int id)
        {
            var enrolment = await _context.Enrolments.FindAsync(id);

            if (enrolment == null)
                return NotFound();

            return Ok(enrolment);
        }

        // POST: api/Enrolments
        [HttpPost]
        public async Task<ActionResult<Enrolment>> CreateEnrolment(Enrolment enrolment)
        {
            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEnrolment),
                new { id = enrolment.EnrolmentID },
                enrolment
            );
        }

        // PUT: api/Enrolments/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEnrolment(int id, Enrolment enrolment)
        {
            if (id != enrolment.EnrolmentID)
                return BadRequest("Enrolment ID does not match.");

            var existingEnrolment = await _context.Enrolments.FindAsync(id);

            if (existingEnrolment == null)
                return NotFound();

            existingEnrolment.ParticipantID = enrolment.ParticipantID;
            existingEnrolment.EventID = enrolment.EventID;
            existingEnrolment.CategoryID = enrolment.CategoryID;
            existingEnrolment.EnrolmentDate = enrolment.EnrolmentDate;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Enrolments/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrolment(int id)
        {
            var enrolment = await _context.Enrolments.FindAsync(id);

            if (enrolment == null)
                return NotFound();

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
