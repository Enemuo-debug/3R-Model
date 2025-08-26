using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using NiRAProject.Tools;

namespace NiRAProject.Dtos
{
    public class LoginDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public required string Email { get; set; }
        [Required]
        [DataType(DataType.Password, ErrorMessage = "Invalid password format.")]
        public required string Password { get; set; }
    }
}