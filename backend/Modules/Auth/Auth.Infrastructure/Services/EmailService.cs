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

            var body = BuildEmailHtml(
                eyebrow: "account setup",
                heading: "Verify your email",
                description: "Thank you for registering at PcBuilder. Click the button below to verify your email address and activate your account.",
                actionUrl: verificationLink,
                actionLabel: "Verify Email",
                footer: "This link expires in 24 hours. If you did not create a PcBuilder account, you can safely ignore this email."
            );

            await SendAsync(toEmail, "Verify your PcBuilder email", body, cancellationToken);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default)
        {
            var frontendUrl = _configuration["Email:FrontendUrl"];
            var resetLink = $"{frontendUrl}/reset-password?token={resetToken}";

            var body = BuildEmailHtml(
                eyebrow: "security",
                heading: "Reset your password",
                description: "You requested a password reset for your PcBuilder account. Click the button below to choose a new password.",
                actionUrl: resetLink,
                actionLabel: "Reset Password",
                footer: "This link expires in 1 hour. If you did not request a password reset, you can safely ignore this email."
            );

            await SendAsync(toEmail, "Reset your PcBuilder password", body, cancellationToken);
        }

        public async Task SendAccountDeletionEmailAsync(string toEmail, CancellationToken cancellationToken = default)
        {
            var body = BuildEmailHtml(
                eyebrow: "account",
                heading: "Account deleted",
                description: "Your PcBuilder account has been deleted by an administrator. All your builds, reviews, and personal data have been removed.",
                actionUrl: null,
                actionLabel: null,
                footer: "If you believe this was done in error, please contact our support team."
            );

            await SendAsync(toEmail, "Your PcBuilder account has been deleted", body, cancellationToken);
        }

        private static string BuildEmailHtml(
            string eyebrow,
            string heading,
            string description,
            string? actionUrl,
            string? actionLabel,
            string footer)
        {
            var actionBlock = actionUrl is not null ? $"""
                <tr>
                  <td style="padding:0 24px 24px;">
                    <a href="{actionUrl}"
                       style="display:inline-block;font-family:'IBM Plex Sans',Arial,sans-serif;font-size:13px;font-weight:600;letter-spacing:0.04em;color:#08080A;background:#AECF55;border:1px solid #AECF55;border-radius:2px;padding:11px 20px;text-decoration:none;">
                      {actionLabel}
                    </a>
                  </td>
                </tr>
                """ : "";

            return $"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                  <meta charset="UTF-8" />
                  <meta name="viewport" content="width=device-width,initial-scale=1" />
                  <title>{heading}</title>
                </head>
                <body style="margin:0;padding:0;background-color:#08080A;background-image:linear-gradient(to right,#1F1F24 1px,transparent 1px),linear-gradient(to bottom,#1F1F24 1px,transparent 1px);background-size:32px 32px;font-family:'IBM Plex Sans',Arial,sans-serif;">
                  <table width="100%" cellpadding="0" cellspacing="0" border="0">
                    <tr>
                      <td align="center" style="padding:40px 20px;">

                        <!-- width cap -->
                        <table width="100%" cellpadding="0" cellspacing="0" border="0" style="max-width:480px;">

                          <!-- wordmark -->
                          <tr>
                            <td style="padding-bottom:20px;">
                              <span style="font-family:'IBM Plex Mono','Courier New',monospace;font-size:16px;font-weight:600;letter-spacing:-0.02em;color:#ECECEE;">
                                <span style="color:#4A4A52;">[</span>PC<span style="color:#AECF55;">/</span>BUILDER<span style="color:#4A4A52;">]</span>
                              </span>
                            </td>
                          </tr>

                          <!-- card -->
                          <tr>
                            <td style="background:#0F0F12;border:1px solid #2E2E36;border-radius:2px;">
                              <table width="100%" cellpadding="0" cellspacing="0" border="0">

                                <!-- card header -->
                                <tr>
                                  <td style="padding:20px 24px 16px;border-bottom:1px solid #1F1F24;">
                                    <span style="display:block;font-family:'IBM Plex Mono','Courier New',monospace;font-size:11px;font-weight:500;letter-spacing:0.14em;text-transform:uppercase;color:#70707A;margin-bottom:8px;">
                                      {eyebrow}
                                    </span>
                                    <span style="display:block;font-family:'IBM Plex Sans',Arial,sans-serif;font-size:26px;font-weight:600;letter-spacing:-0.01em;color:#ECECEE;line-height:1.15;">
                                      {heading}
                                    </span>
                                  </td>
                                </tr>

                                <!-- card body -->
                                <tr>
                                  <td style="padding:20px 24px;">
                                    <p style="margin:0;font-size:14px;line-height:1.5;color:#A6A6AE;">
                                      {description}
                                    </p>
                                  </td>
                                </tr>

                                <!-- CTA button -->
                                {actionBlock}

                                <!-- card footer -->
                                <tr>
                                  <td style="padding:16px 24px;border-top:1px solid #1F1F24;">
                                    <p style="margin:0;font-family:'IBM Plex Mono','Courier New',monospace;font-size:11px;letter-spacing:0.04em;color:#70707A;line-height:1.5;">
                                      {footer}
                                    </p>
                                  </td>
                                </tr>

                              </table>
                            </td>
                          </tr>

                          <!-- bottom meta -->
                          <tr>
                            <td style="padding-top:20px;">
                              <p style="margin:0;font-family:'IBM Plex Mono','Courier New',monospace;font-size:10px;letter-spacing:0.04em;color:#4A4A52;">
                                PcBuilder &mdash; automated message, do not reply
                              </p>
                            </td>
                          </tr>

                        </table>
                      </td>
                    </tr>
                  </table>
                </body>
                </html>
                """;
        }

        private async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_configuration["Email:FromAddress"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

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
