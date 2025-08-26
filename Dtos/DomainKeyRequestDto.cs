using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NiRAProject.Dtos
{
    public class DomainKeyRequestDto
    {
        [Required]
        public int TTL { get; set; }
    }
}