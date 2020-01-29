using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using webservice.Model;

namespace webservice.Models
{
    public class AnchorsContext: DbContext
    {


        public AnchorsContext(DbContextOptions<AnchorsContext> options)
           : base(options)
        {
        }

        public DbSet<Anchor> Anchors { get; set; }

        public String test()
        {
            return "";
        }
    }
}
