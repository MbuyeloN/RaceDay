using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            return await _context.Events.ToListAsync();
        }

        // GET: api/Events/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var raceEvent = await _context.Events.FindAsync(id);

            if (raceEvent == null)
            {
                return NotFound();
            }

            return Ok(raceEvent);
        }

        // POST: api/Events
        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(Event raceEvent)
        {
            _context.Events.Add(raceEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = raceEvent.EventID },
                raceEvent
            );
        }

        // PUT: api/Events/4
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, Event raceEvent)
        {
            if (id != raceEvent.EventID)
            {
                return BadRequest("Event ID does not match.");
            }

            var existingEvent = await _context.Events.FindAsync(id);

            if (existingEvent == null)
            {
                return NotFound();
            }

            existingEvent.OrganiserID = raceEvent.OrganiserID;
            existingEvent.EventTypeID = raceEvent.EventTypeID;
            existingEvent.EventName = raceEvent.EventName;
            existingEvent.Description = raceEvent.Description;
            existingEvent.EventDate = raceEvent.EventDate;
            existingEvent.Location = raceEvent.Location;
            existingEvent.Distance = raceEvent.Distance;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Events/4
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var raceEvent = await _context.Events.FindAsync(id);

            if (raceEvent == null)
            {
                return NotFound();
            }

            _context.Events.Remove(raceEvent);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
