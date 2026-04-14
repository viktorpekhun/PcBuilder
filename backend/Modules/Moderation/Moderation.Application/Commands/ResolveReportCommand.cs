using MediatR;
using Moderation.Domain.Enums;
using PcBuilder.SharedKernel;

namespace Moderation.Application.Commands
{
    public record ResolveReportCommand(
        Guid AdminId,
        Guid ReportId,
        ReportResolutionAction Action,
        string? Reason = null,
        BanType? BanType = null,
        int? BanDurationDays = null) : IRequest<Result<bool>>;
}