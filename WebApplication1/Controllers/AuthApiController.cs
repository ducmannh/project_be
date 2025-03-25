using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Entity;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthApiController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterDto request)
        {
            var user = await authService.RegisterAsync(request);

            if (user is null)
                return BadRequest("User already exists");

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login(LoginDto request)
        {
            var result = await authService.LoginAsync(request);

            if (result is null)
                return BadRequest("Invalid username, email or password");

            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto request)
        {
            var result = await authService.RefreshTokenAsync(request);

            if (result is null || result.AccessToken is null || result.RefreshToken is null)
                return BadRequest("Invalid refresh token");

            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto request)
        {
            var result = await authService.ForgotPassword(request);
            return Ok(new { message = "Nếu email tồn tại, chúng tôi đã gửi mã xác nhận." });
        }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode(VerifyCodeDto request)
        {
            var result = await authService.VerifyCode(request);
            if (!result)
            {
                return BadRequest(new { message = "Mã xác nhận không hợp lệ hoặc đã hết hạn." });
            }
            return Ok(new { message = "Mã xác nhận hợp lệ. Vui lòng nhập mật khẩu mới." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto request)
        {
            var result = await authService.ResetPassword(request);
            if (!result)
            {
                return BadRequest(new { message = "Yêu cầu không hợp lệ. Vui lòng thử lại." });
            }
            return Ok(new { message = "Đặt lại mật khẩu thành công." });
        }

        [Authorize]
        [HttpGet]
        public IActionResult AuthenticatedOnlyEndpoint()
        {
            return Ok("Authenticated");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnlyEndpoint()
        {
            return Ok("Admin");
        }
    }
}