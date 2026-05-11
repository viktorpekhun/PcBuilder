using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moderation.Application.Commands;
using Moderation.Domain.Enums;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;
using PcBuilder.SharedKernel.Services;
using PcBuilds.Application.Commands;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class PublishPcBuildCommandHandler : IRequestHandler<PublishPcBuildCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ITextModerationService _textModeration;
        private readonly ISender _sender;

        public PublishPcBuildCommandHandler(IApplicationDbContext context, ITextModerationService textModeration, ISender sender)
        {
            _context = context;
            _textModeration = textModeration;
            _sender = sender;
        }

        public async Task<Result<bool>> Handle(PublishPcBuildCommand request, CancellationToken cancellationToken)
        {
            var build = await _context.Set<PcBuild>()
                .FirstOrDefaultAsync(b => b.Id == request.PcBuildId, cancellationToken);

            if (build == null)
                return Result.Failure<bool>(new Error("NotFound", "Build not found.", 404));

            if (build.UserId != request.UserId)
                return Result.Failure<bool>(new Error("Forbidden", "You do not have permission to publish this build.", 403));

            if (request.IsPublished)
            {
                var user = await _context.Set<User>()
                    .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
                if (user != null && user.PostBanUntil > DateTime.UtcNow)
                    return Result.Failure<bool>(new Error("Forbidden", "You are temporarily banned from posting.", 403));

                var nameCheck = await _textModeration.CheckAsync(build.Name, cancellationToken: cancellationToken);
                var descCheck = !string.IsNullOrWhiteSpace(build.Description)
                    ? await _textModeration.CheckAsync(build.Description, cancellationToken: cancellationToken)
                    : null;

                if (nameCheck.IsToxic || (descCheck?.IsToxic ?? false))
                {
                    build.IsPublished = false;
                    build.PublishedAt = null;
                    build.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(cancellationToken);

                    await _sender.Send(new IssueWarningCommand(
                        null,
                        request.UserId,
                        BanType.Post,
                        "Automated warning: toxic build content"), cancellationToken);

                    return Result.Failure<bool>(new Error("Toxic", "Build rejected by moderation.", 400));
                }
            }

            build.IsPublished = request.IsPublished;
            build.PublishedAt = request.IsPublished ? DateTime.UtcNow : null;
            build.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(request.IsPublished);
        }
    }
}
