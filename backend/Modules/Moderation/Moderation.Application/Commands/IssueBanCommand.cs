using MediatR;
using Moderation.Domain.Enums;
using PcBuilder.SharedKernel;

namespace Moderation.Application.Commands
{
    public record IssueBanCommand(Guid AdminId, Guid UserId, BanType BanType, int DurationDays, string Reason) : IRequest<Result<bool>>;
}