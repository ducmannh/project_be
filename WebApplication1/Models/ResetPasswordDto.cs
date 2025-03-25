using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class ResetPasswordDto
    {
        public string? Email { get; set; }
        public string? NewPassword { get; set; }
    }
}