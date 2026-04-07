using Auth.Application.Commands;
using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Auth.Application.Handlers
{
    public class SetPasswordCommandHandler : IRequestHandler<SetPasswordCommand, Result<string>>
    {
        private readonly IApplicationDbContext _context;

        public SetPasswordCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<string>> Handle(SetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                return Result<string>.Failure(new Error("NotFound", "User not found.", 404));

            if (!string.IsNullOrEmpty(user.PasswordHash))
                return Result<string>.Failure(new Error("PasswordExists", "This account already has a password. Use change-password instead.", 400));

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            _context.Set<User>().Update(user);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success("Password set successfully.");
        }
    }
}
