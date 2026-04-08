using Auth.Application.Commands;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Auth.Application.Handlers
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<string>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public ForgotPasswordCommandHandler(IApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<Result<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            // Always return success — never leak whether an email is registered
            if (user != null)
            {
                user.PasswordResetToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
                user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

                _context.Set<User>().Update(user);
                await _context.SaveChangesAsync(cancellationToken);

                await _emailService.SendPasswordResetEmailAsync(user.Email, user.PasswordResetToken, cancellationToken);
            }

            return Result<string>.Success("If this email is registered, you will receive a password reset link.");
        }
    }
}
