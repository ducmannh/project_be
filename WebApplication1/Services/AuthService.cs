using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Data;
using WebApplication1.Entity;
using WebApplication1.Models;
using MailKit.Net.Smtp;
using MimeKit;

namespace WebApplication1.Services
{
    public class AuthService(AppDbContext context, IConfiguration configuration, IEmailService emailService) : IAuthService
    {
        // private readonly IEmailService emailService;
        public async Task<TokenResponseDto?> LoginAsync(LoginDto request)
        {
            bool isEmail = request.LoginInput.Contains("@");
            var user = await context.Users.FirstOrDefaultAsync(u => isEmail ? u.Email == request.LoginInput : u.Username == request.LoginInput);
            if (user is null)
                return null;

            var result = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                return null;

            return await CreateTokenResponse(user);
        }

        public async Task<User?> RegisterAsync(RegisterDto request)
        {
            bool userExists = await context.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email);
            if (userExists)
            {
                return null;
            }

            var user = new User();
            var hashedPassword = new PasswordHasher<User>().HashPassword(user, request.Password);

            user.Username = request.Username;
            user.PasswordHash = hashedPassword;
            user.Email = request.Email;

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }

        public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var user = await ValidateRefreshTokenAsync(request.RefreshToken);
            if (user is null)
                return null;

            return await CreateTokenResponse(user);
        }

        public async Task<bool> ForgotPassword(ForgotPasswordDto request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return true; 
            }

            var random = new Random();
            var resetCode = random.Next(100000, 999999).ToString();
            user.ResetCode = resetCode;
            user.ResetCodeExpiry = DateTime.UtcNow.AddMinutes(15);
            await context.SaveChangesAsync();

            await emailService.SendResetCodeEmail(user.Email, resetCode);
            return true;
        }

        public async Task<bool> VerifyCode(VerifyCodeDto request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || user.ResetCode != request.Code || user.ResetCodeExpiry < DateTime.UtcNow)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> ResetPassword(ResetPasswordDto request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || string.IsNullOrEmpty(user.ResetCode))
            {
                return false;
            }
            var hashedPassword = new PasswordHasher<User>().HashPassword(user, request.NewPassword);

            user.PasswordHash = hashedPassword;
            user.ResetCode = null;
            user.ResetCodeExpiry = null;
            await context.SaveChangesAsync();
            return true;
        }

        private async Task<TokenResponseDto> CreateTokenResponse(User user)
        {
            return new TokenResponseDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
            };
        }

        private async Task<User?> ValidateRefreshTokenAsync(string refreshToken)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
            if (user is null || user.RefreshTokenExpiration <= DateTime.UtcNow || user.RefreshToken != refreshToken)
                return null;

            return user;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiration = DateTime.Now.AddDays(7);
            await context.SaveChangesAsync();
            return refreshToken;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                // new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                // new Claim(ClaimTypes.Role, user.Role)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}