using MediatR;
using Microsoft.EntityFrameworkCore;
using Moderation.Application.Commands;
using Moderation.Application.Services;
using Moderation.Domain.Entities;
using Moderation.Domain.Enums;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Domain.Entities;

namespace Moderation.Application.Handlers
{
    public class ResolveReportCommandHandler : IRequestHandler<ResolveReportCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ISender _sender;
        private readonly IAdminActivityLogger _activity;

        public ResolveReportCommandHandler(IApplicationDbContext context, ISender sender, IAdminActivityLogger activity)
        {
            _context = context;
            _sender = sender;
            _activity = activity;
        }

        public async Task<Result<bool>> Handle(ResolveReportCommand request, CancellationToken cancellationToken)
        {
            var report = await _context.Set<Report>()
                .FirstOrDefaultAsync(r => r.Id == request.ReportId, cancellationToken);

            if (report == null)
                return Result.Failure<bool>(new Error("NotFound", "Report not found.", 404));

            if (report.Status != ReportStatus.Pending)
                return Result.Failure<bool>(new Error("Conflict", "Report has already been resolved.", 409));

            var contentStillExists = await ReportedContentExists(report, cancellationToken);

            if (!contentStillExists &&
                (request.Action == ReportResolutionAction.DeleteContent ||
                 request.Action == ReportResolutionAction.DeleteContentAndWarn ||
                 request.Action == ReportResolutionAction.DeleteContentAndBan))
            {
                return Result.Failure<bool>(new Error("Conflict",
                    "Reported content no longer exists. Dismiss the report or choose a non-delete action.", 409));
            }

            Result<bool>? actionResult = null;

            switch (request.Action)
            {
                case ReportResolutionAction.Dismiss:
                    break;

                case ReportResolutionAction.DeleteContent:
                    actionResult = await DeleteReportedContent(report, request.AdminId, cancellationToken);
                    break;

                case ReportResolutionAction.DeleteContentAndWarn:
                    actionResult = await DeleteReportedContent(report, request.AdminId, cancellationToken);
                    if (actionResult.IsFailure) return actionResult;
                    var warnBanType = request.BanType
                        ?? (report.ReportType == ReportType.Review ? BanType.Comment : BanType.Post);
                    var warnResult = await _sender.Send(new IssueWarningCommand(
                        request.AdminId,
                        report.ReportedUserId,
                        warnBanType,
                        request.ReasonCode ?? WarnReasonCodes.CommunityGuidelines), cancellationToken);
                    if (warnResult.IsFailure)
                        return Result.Failure<bool>(warnResult.Error!);
                    break;

                case ReportResolutionAction.DeleteContentAndBan:
                    actionResult = await DeleteReportedContent(report, request.AdminId, cancellationToken);
                    if (actionResult.IsFailure) return actionResult;
                    var banType = request.BanType
                        ?? (report.ReportType == ReportType.Review ? BanType.Comment : BanType.Post);
                    var banResult = await _sender.Send(new IssueBanCommand(
                        request.AdminId,
                        report.ReportedUserId,
                        banType,
                        request.BanDurationDays ?? 1,
                        request.Reason ?? report.Reason), cancellationToken);
                    if (banResult.IsFailure)
                        return Result.Failure<bool>(banResult.Error!);
                    break;
            }

            if (actionResult?.IsFailure == true)
                return actionResult;

            var newStatus = request.Action == ReportResolutionAction.Dismiss
                ? ReportStatus.Dismissed
                : ReportStatus.Resolved;

            report.Status = newStatus;
            report.ResolvedAt = DateTime.UtcNow;
            report.ResolvedByAdminId = request.AdminId;
            report.AdminResolutionNote = request.Reason;

            await _context.SaveChangesAsync(cancellationToken);

            await _activity.LogAsync(
                request.AdminId,
                newStatus == ReportStatus.Dismissed ? "DismissReport" : "ResolveReport",
                targetType: "Report",
                targetId: report.Id,
                targetName: $"{report.ReportType} by @{report.ReportedUserId}",
                detail: request.Action.ToString(),
                cancellationToken: cancellationToken);

            return Result.Success(true);
        }

        private async Task<bool> ReportedContentExists(Report report, CancellationToken cancellationToken)
        {
            return report.ReportType == ReportType.Review
                ? await _context.Set<Review>().AnyAsync(r => r.Id == report.ReportedEntityId, cancellationToken)
                : await _context.Set<PcBuild>().AnyAsync(b => b.Id == report.ReportedEntityId, cancellationToken);
        }

        private async Task<Result<bool>> DeleteReportedContent(Report report, Guid adminId, CancellationToken cancellationToken)
        {
            if (report.ReportType == ReportType.Review)
            {
                return await _sender.Send(new AdminDeleteReviewCommand(
                    adminId,
                    report.ReportedEntityId,
                    NotifyUser: true), cancellationToken);
            }
            else
            {
                return await _sender.Send(new AdminDeleteBuildCommand(
                    adminId,
                    report.ReportedEntityId,
                    NotifyUser: true), cancellationToken);
            }
        }
    }
}