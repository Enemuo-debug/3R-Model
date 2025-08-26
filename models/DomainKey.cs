using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NiRAProject.models
{
    public class DomainKey
    {
        public int Id { get; set; }
        public required string IssuerId { get; set; }
        public DateTime TTL { get; set; } = DateTime.UtcNow.AddDays(30);
        public required string OwnerId { get; set; }
        public bool Active { get; set; } = false;
    }
}