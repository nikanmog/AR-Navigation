using Microsoft.EntityFrameworkCore;
using webservice.Model;

namespace webservice.Models
{
    public class AnchorsContext: DbContext
    {
        public AnchorsContext(DbContextOptions<AnchorsContext> options) : base(options) { }
        public DbSet<Anchor> Anchors { get; set; }
    }
}
