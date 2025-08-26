using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NiRAProject.models
{
    public class Domain
    {
        public int Id { get; set; }
        public required string domainName { get; set; }
        public required int domainTypeId { get; set; }
        public required string OwnerId { get; set; }
        public bool Active { get; set; } = false;
    }
}