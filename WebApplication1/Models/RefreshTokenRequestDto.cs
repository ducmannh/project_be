using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class RefreshTokenRequestDto
    {
        // public Guid UserId { get; set; }
        public required string RefreshToken { get; set; }
    }
}