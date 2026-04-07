using Auth.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Auth.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendVerificationEmailAsync(string toEmail, string verificationToken, CancellationToken cancellationToken = default)
        {
            var frontendUrl = _configuration["Email:FrontendUrl"];
            var verificationLink = $"{frontendUrl}/verify-email?token={verificationToken}";

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_configuration["Email:FromAddress"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Verify your PcBuilder email";
            message.Body = new TextPart("html")
            {
                Text = $"""
                    <h2>Email Verification</h2>
                    <p>Thank you for registering at PcBuilder!</p>
                    <p>Please click the link below to verify your email address:</p>
                    <p><a href="{verificationLink}">Verify Email</a></p>
                    <p>This link expires in 24 hours.</p>
                    <p>If you did not create an account, you can safely ignore this email.</p>
                    """
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _configuration["Email:Smtp:Host"],
                _configuration.GetValue<int>("Email:Smtp:Port"),
                SecureSocketOptions.StartTls,
                cancellationToken);

            await client.AuthenticateAsync(
                _configuration["Email:Smtp:Username"],
                _configuration["Email:Smtp:Password"],
                cancellationToken);

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
    }
}
