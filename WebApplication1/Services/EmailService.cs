using MailKit.Net.Smtp;
using MimeKit;

namespace WebApplication1.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendResetCodeEmail(string email, string resetCode)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_configuration["EmailSettings:SenderName"], _configuration["EmailSettings:SenderEmail"]));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Mã xác nhận đặt lại mật khẩu";

            message.Body = new TextPart("plain")
            {
                Text = $"Mã xác nhận của bạn là: {resetCode}\nMã này có hiệu lực trong 15 phút."
            };

            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync(_configuration["EmailSettings:SmtpServer"], int.Parse(_configuration["EmailSettings:Port"]), MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_configuration["EmailSettings:SenderEmail"], _configuration["EmailSettings:Password"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}