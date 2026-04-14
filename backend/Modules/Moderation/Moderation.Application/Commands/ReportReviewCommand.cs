using MediatR;
using PcBuilder.SharedKernel;

namespace Moderation.Application.Commands
{
    public record ReportReviewCommand(Guid ReporterId, Guid ReviewId, string Reason) : IRequest<Result<Guid>>;
}