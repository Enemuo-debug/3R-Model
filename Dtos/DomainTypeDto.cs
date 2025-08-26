using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NiRAProject.Dtos
{
    public class DomainTypeDto
    {
        [Required]
        [MinLength(2, ErrorMessage = "Domain suffix must be at least 2 characters long.")]
        [MaxLength(5, ErrorMessage = "Domain suffix cannot exceed 10 characters.")]
        [RegularExpression("^[A-Za-z]+$", ErrorMessage = "Domain suffix cannot contain spaces or special characters.")]
        public required string _domainSuffix { get; set; }
        public int SuperDomain { get; set; }
    }
}