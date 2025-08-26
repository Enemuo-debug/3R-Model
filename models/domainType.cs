using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NiRAProject.models
{
    public class domainType
    {
        public int Id { get; set; }
        public required string _domainSuffix { get; set; }
        public required int SuperDomain { get; set; }
    }
}