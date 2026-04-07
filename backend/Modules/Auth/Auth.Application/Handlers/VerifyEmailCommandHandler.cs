using Auth.Application.Commands;
using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Auth.Application.Handlers
{
    public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result<string>>
    {
        private readonly IApplicationDbContext _context;

        public VerifyEmailCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<string>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == request.Token, cancellationToken);

            if (user == null)
            {
                return Result<string>.Failure(
                    new Error("InvalidToken", "Invalid verification token.", 400));
            }

            if (user.EmailVerificationTokenExpiry <= DateTime.UtcNow)
            {
                return Result<string>.Failure(
                    new Error("ExpiredToken", "Verification token has expired.", 400));
            }

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;

            _context.Set<User>().Update(user);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success("Email verified successfully.");
        }
    }
}
