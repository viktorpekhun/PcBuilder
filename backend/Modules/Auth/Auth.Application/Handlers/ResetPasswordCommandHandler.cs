using Auth.Application.Commands;
using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Auth.Application.Handlers
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<string>>
    {
        private readonly IApplicationDbContext _context;

        public ResetPasswordCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token, cancellationToken);

            if (user == null)
            {
                return Result<string>.Failure(
                    new Error("InvalidToken", "Password reset link is invalid or has already been used.", 400));
            }

            if (user.PasswordResetTokenExpiry <= DateTime.UtcNow)
            {
                return Result<string>.Failure(
                    new Error("ExpiredToken", "Password reset link has expired.", 400));
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            _context.Set<User>().Update(user);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success("Password reset successfully.");
        }
    }
}
