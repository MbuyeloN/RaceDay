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

        // GET: api/Results
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Result>>> GetResults()
        {
            return await _context.Results.ToListAsync();
        }

        // GET: api/Results/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Result>> GetResult(int id)
        {
            var result = await _context.Results.FindAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // POST: api/Results
        [HttpPost]
        public async Task<ActionResult<Result>> CreateResult(Result result)
        {
            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetResult),
                new { id = result.ResultID },
                result
            );
        }

        // PUT: api/Results/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResult(int id, Result result)
        {
            if (id != result.ResultID)
                return BadRequest("Result ID does not match.");

            var existingResult = await _context.Results.FindAsync(id);

            if (existingResult == null)
                return NotFound();

            existingResult.EnrolmentID = result.EnrolmentID;
            existingResult.FinishTime = result.FinishTime;
            existingResult.FinishingPosition = result.FinishingPosition;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Results/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var result = await _context.Results.FindAsync(id);

            if (result == null)
                return NotFound();

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
