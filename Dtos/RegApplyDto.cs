using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace NiRAProject.Dtos
{
    public class RegApplyDto
    {
        [Required]
        [MinLength(10, ErrorMessage = "User name is too short")]
        [MaxLength(30, ErrorMessage = "User name is too long")]
        public required string UserName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public required string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{6,}$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public required string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}