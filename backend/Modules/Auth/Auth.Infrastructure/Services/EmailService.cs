using Auth.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Auth.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmailService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetLanguage()
        {
            var feature = _httpContextAccessor.HttpContext?
                .Features.Get<IRequestCultureFeature>();
            var culture = feature?.RequestCulture.UICulture.TwoLetterISOLanguageName ?? "en";
            return culture == "uk" ? "uk" : "en";
        }

        public async Task SendVerificationEmailAsync(string toEmail, string verificationToken, CancellationToken cancellationToken = default)
        {
            var lang = GetLanguage();
            var frontendUrl = _configuration["Email:FrontendUrl"];
            var verificationLink = $"{frontendUrl}/verify-email?token={verificationToken}";

            var (subject, eyebrow, heading, description, actionLabel, footer) = lang == "uk"
                ? (
                    "Підтвердіть ваш email в PcBuilder",
                    "налаштування акаунту",
                    "Підтвердження email",
                    "Дякуємо за реєстрацію в PcBuilder. Натисніть кнопку нижче, щоб підтвердити адресу електронної пошти та активувати акаунт.",
                    "Підтвердити Email",
                    "Посилання дійсне 24 години. Якщо ви не створювали акаунт в PcBuilder, просто проігноруйте цей лист."
                )
                : (
                    "Verify your PcBuilder email",
                    "account setup",
                    "Verify your email",
                    "Thank you for registering at PcBuilder. Click the button below to verify your email address and activate your account.",
                    "Verify Email",
                    "This link expires in 24 hours. If you did not create a PcBuilder account, you can safely ignore this email."
                );

            var body = BuildEmailHtml(lang, eyebrow, heading, description, verificationLink, actionLabel, footer);
            await SendAsync(toEmail, subject, body, cancellationToken);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default)
        {
            var lang = GetLanguage();
            var frontendUrl = _configuration["Email:FrontendUrl"];
            var resetLink = $"{frontendUrl}/reset-password?token={resetToken}";

            var (subject, eyebrow, heading, description, actionLabel, footer) = lang == "uk"
                ? (
                    "Скидання пароля PcBuilder",
                    "безпека",
                    "Скидання пароля",
                    "Ви запросили скидання пароля для вашого акаунту PcBuilder. Натисніть кнопку нижче, щоб встановити новий пароль.",
                    "Скинути пароль",
                    "Посилання дійсне 1 годину. Якщо ви не запитували скидання пароля, просто проігноруйте цей лист."
                )
                : (
                    "Reset your PcBuilder password",
                    "security",
                    "Reset your password",
                    "You requested a password reset for your PcBuilder account. Click the button below to choose a new password.",
                    "Reset Password",
                    "This link expires in 1 hour. If you did not request a password reset, you can safely ignore this email."
                );

            var body = BuildEmailHtml(lang, eyebrow, heading, description, resetLink, actionLabel, footer);
            await SendAsync(toEmail, subject, body, cancellationToken);
        }

        public async Task SendAccountDeletionEmailAsync(string toEmail, string language = "en", CancellationToken cancellationToken = default)
        {
            var lang = language == "uk" ? "uk" : "en";

            var (subject, eyebrow, heading, description, footer) = lang == "uk"
                ? (
                    "Ваш акаунт PcBuilder видалено",
                    "акаунт",
                    "Акаунт видалено",
                    "Ваш акаунт PcBuilder був видалений адміністратором. Усі ваші збірки, відгуки та особисті дані було видалено.",
                    "Якщо ви вважаєте, що це сталося помилково, будь ласка, зв'яжіться з нашою службою підтримки."
                )
                : (
                    "Your PcBuilder account has been deleted",
                    "account",
                    "Account deleted",
                    "Your PcBuilder account has been deleted by an administrator. All your builds, reviews, and personal data have been removed.",
                    "If you believe this was done in error, please contact our support team."
                );

            var body = BuildEmailHtml(lang, eyebrow, heading, description, null, null, footer);
            await SendAsync(toEmail, subject, body, cancellationToken);
        }

        private static string BuildEmailHtml(
            string lang,
            string eyebrow,
            string heading,
            string description,
            string? actionUrl,
            string? actionLabel,
            string footer)
        {
            var htmlLang = lang == "uk" ? "uk" : "en";
            var bottomMeta = lang == "uk"
                ? "PcBuilder &mdash; автоматичне повідомлення, не відповідайте"
                : "PcBuilder &mdash; automated message, do not reply";

            var actionBlock = actionUrl is not null ? $"""
                <tr>
                  <td style="padding:0 24px 24px;">
                    <a href="{actionUrl}"
                       style="display:inline-block;font-family:'IBM Plex Sans',Arial,sans-serif;font-size:13px;font-weight:600;letter-spacing:0.04em;color:#FFFFFF;background:#5A7A1A;border:1px solid #5A7A1A;border-radius:2px;padding:11px 20px;text-decoration:none;">
                      {actionLabel}
                    </a>
                  </td>
                </tr>
                """ : "";

            return $"""
                <!DOCTYPE html>
                <html lang="{htmlLang}">
                <head>
                  <meta charset="UTF-8" />
                  <meta name="viewport" content="width=device-width,initial-scale=1" />
                  <title>{heading}</title>
                </head>
                <body style="margin:0;padding:0;background-color:#F4F4F6;background-image:linear-gradient(to right,#E2E2E7 1px,transparent 1px),linear-gradient(to bottom,#E2E2E7 1px,transparent 1px);background-size:32px 32px;font-family:'IBM Plex Sans',Arial,sans-serif;">
                  <table width="100%" cellpadding="0" cellspacing="0" border="0">
                    <tr>
                      <td align="center" style="padding:40px 20px;">

                        <!-- width cap -->
                        <table width="100%" cellpadding="0" cellspacing="0" border="0" style="max-width:480px;">

                          <!-- wordmark -->
                          <tr>
                            <td style="padding-bottom:20px;">
                              <span style="font-family:'IBM Plex Mono','Courier New',monospace;font-size:16px;font-weight:600;letter-spacing:-0.02em;color:#111114;">
                                <span style="color:#A6A6B0;">[</span>pcbuilder<span style="color:#5A7A1A;">/</span><span style="color:#A6A6B0;">]</span>
                              </span>
                            </td>
                          </tr>

                          <!-- card -->
                          <tr>
                            <td style="background:#FFFFFF;border:1px solid #CECED4;border-radius:2px;">
                              <table width="100%" cellpadding="0" cellspacing="0" border="0">

                                <!-- card header -->
                                <tr>
                                  <td style="padding:20px 24px 16px;border-bottom:1px solid #E2E2E7;">
                                    <span style="display:block;font-family:'IBM Plex Mono','Courier New',monospace;font-size:11px;font-weight:500;letter-spacing:0.14em;text-transform:uppercase;color:#717179;margin-bottom:8px;">
                                      {eyebrow}
                                    </span>
                                    <span style="display:block;font-family:'IBM Plex Sans',Arial,sans-serif;font-size:26px;font-weight:600;letter-spacing:-0.01em;color:#111114;line-height:1.15;">
                                      {heading}
                                    </span>
                                  </td>
                                </tr>

                                <!-- card body -->
                                <tr>
                                  <td style="padding:20px 24px;">
                                    <p style="margin:0;font-size:14px;line-height:1.5;color:#3e3e45;">
                                      {description}
                                    </p>
                                  </td>
                                </tr>

                                <!-- CTA button -->
                                {actionBlock}

                                <!-- card footer -->
                                <tr>
                                  <td style="padding:16px 24px;border-top:1px solid #E2E2E7;">
                                    <p style="margin:0;font-family:'IBM Plex Mono','Courier New',monospace;font-size:11px;letter-spacing:0.04em;color:#717179;line-height:1.5;">
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
                              <p style="margin:0;font-family:'IBM Plex Mono','Courier New',monospace;font-size:10px;letter-spacing:0.04em;color:#A6A6B0;">
                                {bottomMeta}
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
