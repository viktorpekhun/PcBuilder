using Auth.Application.Commands;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Auth.Application.Handlers
{
    public class ResendVerificationEmailCommandHandler : IRequestHandler<ResendVerificationEmailCommand, Result<string>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public ResendVerificationEmailCommandHandler(
            IApplicationDbContext context,
            IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<Result<string>> Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                return Result<string>.Failure(
                    new Error("UserNotFound", "User not found.", 404));
            }

            if (user.IsEmailVerified)
            {
                return Result<string>.Failure(
                    new Error("AlreadyVerified", "Email is already verified.", 400));
            }

            user.EmailVerificationToken = Guid.NewGuid().ToString("N");
            user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

            _context.Set<User>().Update(user);
            await _context.SaveChangesAsync(cancellationToken);

            await _emailService.SendVerificationEmailAsync(user.Email, user.EmailVerificationToken, cancellationToken);

            return Result<string>.Success("Verification email sent.");
        }
    }
}
