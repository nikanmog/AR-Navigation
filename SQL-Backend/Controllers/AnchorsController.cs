using System.Collections.Generic;

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
        private static string demo = "Default";
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
            List<Anchor> result = await _context.Anchors.ToListAsync();
            List<Anchor> currentDemoAnchors = result.FindAll(
                delegate (Anchor a)
                {
                    return a.Demo.Equals(demo);
                }
            );
            int i = 1;
            currentDemoAnchors.ForEach(
                delegate (Anchor a)
                {
                    a.Id = i;
                    i++;
                }
            );
            return currentDemoAnchors;
        }
        // POST: api/Anchors
        [HttpPost]
        public async Task<ActionResult<Anchor>> PostAsync(Anchor anchor)
        {
            Anchor savedObject = anchor;
            savedObject.Demo = demo;
            _context.Anchors.Add(savedObject);
            await _context.SaveChangesAsync();
            return anchor;
        }
        // POST: api/Anchors
        [HttpPost("setDemo")]
        public string SetDemo(string demo)
        {
            string oldDemo = demo;
            AnchorsController.demo = demo;
            return "Changed Demo From " + oldDemo + " to " + demo;
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
