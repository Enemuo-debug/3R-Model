using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NiRAProject.models
{
    public class RegistrarApplicationModel
    {
        public int Id { get; set; }

        public required string UserName { get; set; }
        
        public required string Email { get; set; }

        public required string Password { get; set; }

        public bool IsApproved { get; set; } = false;
    }
}