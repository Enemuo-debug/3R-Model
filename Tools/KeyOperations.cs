using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NiRAProject.Tools
{
    public class KeyOperations
    {
        private readonly IConfiguration _configuration;
        public KeyOperations(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public bool ValidateKey(string key = "")
        {
            var superSecretKey = _configuration["SuperSecrets:Key"];
            return key == superSecretKey;
        }
    }
}