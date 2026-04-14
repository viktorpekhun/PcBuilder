namespace Auth.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string toEmail, string verificationToken, CancellationToken cancellationToken = default);
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default);
        Task SendAccountDeletionEmailAsync(string toEmail, CancellationToken cancellationToken = default);
    }
}
