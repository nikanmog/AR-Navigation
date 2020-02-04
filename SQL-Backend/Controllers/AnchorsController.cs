using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webservice.Model;
using webservice.Models;

namespace webservice.Controllers
{
    [Route("api/anchors")]
    [ApiController]
    public class AnchorsController : ControllerBase
    {
        private readonly AnchorsContext _context;
        public AnchorsController(AnchorsContext context)
        {
            _context = context;
            _context.Database.EnsureCreated();
        }
        // GET: api/Anchors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Anchor>>> GetAnchors()
        {
            return await _context.Anchors.ToListAsync();
        }
        // POST: api/Anchors
        [HttpPost]
        public async Task<ActionResult<Anchor>> PostAsync(Anchor anchor)
        {
            _context.Anchors.Add(anchor);
            await _context.SaveChangesAsync();
            return anchor;
        }
        // DELETE: api/Anchors
        [HttpDelete]
        public void DeleteAnchor()
        {
            _context.Database.ExecuteSqlRaw("drop table Anchors");
            _context.Database.EnsureCreated();
        }
    }
}
