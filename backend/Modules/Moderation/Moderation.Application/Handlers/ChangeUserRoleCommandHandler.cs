using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moderation.Application.Commands;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Moderation.Application.Handlers
{
    public class ChangeUserRoleCommandHandler : IRequestHandler<ChangeUserRoleCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;

        public ChangeUserRoleCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
        {
            if (request.UserId == request.AdminId)
                return Result.Failure<bool>(new Error("Forbidden", "You cannot change your own role.", 403));

            var user = await _context.Set<User>()
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                return Result.Failure<bool>(new Error("NotFound", "User not found.", 404));

            var role = await _context.Set<Role>()
                .FirstOrDefaultAsync(r => r.Name == request.Role, cancellationToken);

            if (role == null)
                return Result.Failure<bool>(new Error("NotFound", $"Role '{request.Role}' not found.", 404));

            var isAlreadyAdmin = user.Roles.Any(r => r.Name == "Admin");

            if (request.Role == "Admin" && !isAlreadyAdmin)
            {
                user.Roles.Add(role);
            }
            else if (request.Role == "User" && isAlreadyAdmin)
            {
                var adminRole = user.Roles.First(r => r.Name == "Admin");
                user.Roles.Remove(adminRole);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success(request.Role == "Admin");
        }
    }
}