using MediatR;
using Moderation.Domain.Enums;
using PcBuilder.SharedKernel;

namespace Moderation.Application.Commands
{
    public record IssueWarningCommand(Guid AdminId, Guid UserId, BanType BanType, string Reason) : IRequest<Result<Guid>>;
}