using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NiRAProject.Dtos
{
    public class SendEmailDto
    {
        public required string Email { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}