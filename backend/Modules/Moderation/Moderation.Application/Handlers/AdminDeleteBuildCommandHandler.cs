using MediatR;
using Microsoft.EntityFrameworkCore;
using Moderation.Application.Commands;
using Notifications.Application.Commands;
using Notifications.Domain;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Domain.Entities;

namespace Moderation.Application.Handlers
{
    public class AdminDeleteBuildCommandHandler : IRequestHandler<AdminDeleteBuildCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ISender _sender;

        public AdminDeleteBuildCommandHandler(IApplicationDbContext context, ISender sender)
        {
            _context = context;
            _sender = sender;
        }

        public async Task<Result<bool>> Handle(AdminDeleteBuildCommand request, CancellationToken cancellationToken)
        {
            var build = await _context.Set<PcBuild>()
                .FirstOrDefaultAsync(b => b.Id == request.BuildId, cancellationToken);

            if (build == null)
                return Result.Failure<bool>(new Error("NotFound", "Build not found.", 404));

            var userId = build.UserId;
            var buildName = build.Name;

            _context.Set<PcBuild>().Remove(build);
            await _context.SaveChangesAsync(cancellationToken);

            if (request.NotifyUser)
            {
                await _sender.Send(new CreateNotificationCommand(
                    userId,
                    NotificationTypes.BuildDeleted,
                    new Dictionary<string, string>
                    {
                        ["buildName"] = buildName
                    }), cancellationToken);
            }

            return Result.Success(true);
        }
    }
}