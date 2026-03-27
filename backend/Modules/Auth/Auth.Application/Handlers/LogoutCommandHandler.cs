using Auth.Application.Commands;
using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel.Persistence;

namespace Auth.Application.Handlers
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
    {
        private readonly IApplicationDbContext _context;

        public LogoutCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return;
            }

            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

            if (user != null)
            {
                user.RefreshToken = "";
                user.RefreshTokenExpiryTime = DateTime.MinValue;
                _context.Set<User>().Update(user);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
