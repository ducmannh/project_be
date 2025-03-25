using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class VerifyCodeDto
    {
        public string? Email { get; set; }
        public string? Code { get; set; }
    }
}