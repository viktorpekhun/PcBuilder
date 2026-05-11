using MediatR;
using Microsoft.EntityFrameworkCore;
using Moderation.Application.Commands;
using Moderation.Domain.Entities;
using Moderation.Domain.Enums;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Domain.Entities;

namespace Moderation.Application.Handlers
{
    public class ReportBuildCommandHandler : IRequestHandler<ReportBuildCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;

        public ReportBuildCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> Handle(ReportBuildCommand request, CancellationToken cancellationToken)
        {
            var build = await _context.Set<PcBuild>()
                .FirstOrDefaultAsync(b => b.Id == request.BuildId, cancellationToken);

            if (build == null)
                return Result.Failure<Guid>(new Error("NotFound", "Build not found.", 404));

            if (build.UserId == request.ReporterId)
                return Result.Failure<Guid>(new Error("Conflict", "You cannot report your own build.", 409));

            var alreadyReported = await _context.Set<Report>()
                .AnyAsync(r => r.ReporterId == request.ReporterId
                    && r.ReportType == ReportType.Build
                    && r.ReportedEntityId == request.BuildId
                    && r.Status == ReportStatus.Pending, cancellationToken);

            if (alreadyReported)
                return Result.Failure<Guid>(new Error("Conflict", "You have already reported this build.", 409));

            var report = new Report
            {
                ReporterId = request.ReporterId,
                ReportType = ReportType.Build,
                ReportedEntityId = request.BuildId,
                ReportedUserId = build.UserId,
                Reason = request.Reason,
                Status = ReportStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Set<Report>().AddAsync(report, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(report.Id);
        }
    }
}