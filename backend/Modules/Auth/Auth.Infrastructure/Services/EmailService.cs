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

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default)
        {
            var frontendUrl = _configuration["Email:FrontendUrl"];
            var resetLink = $"{frontendUrl}/reset-password?token={resetToken}";

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_configuration["Email:FromAddress"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Reset your PcBuilder password";
            message.Body = new TextPart("html")
            {
                Text = $"""
                    <h2>Password Reset</h2>
                    <p>You requested a password reset for your PcBuilder account.</p>
                    <p>Click the link below to reset your password:</p>
                    <p><a href="{resetLink}">Reset Password</a></p>
                    <p>This link expires in 1 hour.</p>
                    <p>If you did not request a password reset, you can safely ignore this email.</p>
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

        public async Task SendAccountDeletionEmailAsync(string toEmail, CancellationToken cancellationToken = default)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_configuration["Email:FromAddress"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Your PcBuilder account has been deleted";
            message.Body = new TextPart("html")
            {
                Text = $"""
                    <h2>Account Deleted</h2>
                    <p>Your PcBuilder account has been deleted by an administrator.</p>
                    <p>All your builds, reviews, and personal data have been removed.</p>
                    <p>If you believe this was done in error, please contact our support team.</p>
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
