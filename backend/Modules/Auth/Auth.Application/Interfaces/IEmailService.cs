namespace Auth.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string toEmail, string verificationToken, CancellationToken cancellationToken = default);
    }
}
