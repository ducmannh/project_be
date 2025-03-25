using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Entity;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(RegisterDto request);
        Task<TokenResponseDto?> LoginAsync(LoginDto request);
        Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<bool> ForgotPassword(ForgotPasswordDto request);
        Task<bool> VerifyCode(VerifyCodeDto request);
        Task<bool> ResetPassword(ResetPasswordDto request);
    }
}