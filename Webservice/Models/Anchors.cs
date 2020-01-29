using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webservice.Model
{
    public class Anchor
    {
            public long Id { get; set; }
            public string demo { get; set; }
            public string AnchorKey { get; set; }
            public long AnchorType { get; set; }
            public DateTime Timestamp { get; set; }
    }
}
