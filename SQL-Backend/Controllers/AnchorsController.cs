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
        [HttpPost]
        public async Task<ActionResult<int>> PostAsync()
        {
            string anchorKey;
            using (StreamReader reader = new StreamReader(this.Request.Body, Encoding.UTF8))
            {
                anchorKey = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(anchorKey))
            {
                return this.BadRequest();
            }

            var anchor = new Anchor();
            anchor.AnchorKey = anchorKey;
            anchor.Demo = "default";
            _context.Anchors.Add(anchor);
            _context.SaveChanges();
            return anchor.Id;
        }
        [HttpDelete]
        public void DeleteAnchor()
        {
            _context.Database.ExecuteSqlRaw("drop table Anchors");
            _context.Database.EnsureCreated();
        }
    }
}
