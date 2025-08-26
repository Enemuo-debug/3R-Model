using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using NiRAProject.Tools;

namespace NiRAProject.Dtos
{
    public class ManagerDto
    {
        [Required]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters long.")]
        [MaxLength(20, ErrorMessage = "Username cannot exceed 20 characters.")]
        public required string UserName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public required string Email { get; set; }
        [Required]
        public string Key { get; set; } = string.Empty;
        [Required]
        public required string Password { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; } = null;
        [Required]
        [EnumDataType(typeof(RRRTypes))]
        public required RRRTypes Role { get; set; }
    }
}