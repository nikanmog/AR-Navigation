using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        [HttpGet("{anchorNumber}/key")]
        public async Task<ActionResult<String>> GetAnchorKey(int anchorNumber)
        {
            var anchor = await _context.Anchors.FindAsync(anchorNumber);

            if (anchor == null)
            {
                return NotFound();
            }

            return anchor.AnchorKey;
        }
        [HttpGet("{anchorNumber}/type")]
        public async Task<ActionResult<int>> GetAnchorType(int anchorNumber)
        {
            var anchor = await _context.Anchors.FindAsync(anchorNumber);
            if (anchor == null)
            {
                return NotFound();
            }
            return anchor.AnchorType;
        }
        [HttpGet("count")]
        public async Task<ActionResult<int>> CountAnchors()
        {
            return await _context.Anchors.CountAsync(); 
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
            anchor.Timestamp = DateTime.Now;
            _context.Anchors.Add(anchor);
            _context.SaveChanges();
            return anchor.Id;
        }
        [HttpDelete]
        public void DeleteAnchor()
        {
            _context.Database.ExecuteSqlRaw("delete from Anchors");
        }
    }
}
