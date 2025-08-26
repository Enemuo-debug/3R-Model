using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NiRAProject.Tools;
using Microsoft.AspNetCore.Identity;

namespace NiRAProject.models
{
    public class RRRModel : IdentityUser
    {
        public RRRTypes Managers { get; set; }
    }
}