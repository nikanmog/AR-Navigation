using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webservice.Model;
using webservice.Models;

namespace webservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnchorsController : ControllerBase
    {
        private readonly AnchorsContext _context;

        public AnchorsController(AnchorsContext context)
        {
            _context = context;
        }

        // GET: api/Anchors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Anchor>>> GetAnchors()
        {
            return await _context.Anchors.ToListAsync();
        }

        // GET: api/Anchors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Anchor>> GetAnchor(long id)
        {
            var anchor = await _context.Anchors.FindAsync(id);

            if (anchor == null)
            {
                return NotFound();
            }

            return anchor;
        }

        // PUT: api/Anchors/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAnchor(long id, Anchor anchor)
        {
            if (id != anchor.Id)
            {
                return BadRequest();
            }

            _context.Entry(anchor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AnchorExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Anchors
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Anchor>> PostAnchor(Anchor anchor)
        {
            _context.Anchors.Add(anchor);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAnchor", new { id = anchor.Id }, anchor);
        }

        // DELETE: api/Anchors/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Anchor>> DeleteAnchor(long id)
        {
            var anchor = await _context.Anchors.FindAsync(id);
            if (anchor == null)
            {
                return NotFound();
            }

            _context.Anchors.Remove(anchor);
            await _context.SaveChangesAsync();

            return anchor;
        }

        private bool AnchorExists(long id)
        {
            return _context.Anchors.Any(e => e.Id == id);
        }
    }
}
