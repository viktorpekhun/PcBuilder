using MediatR;
using Moderation.Domain.Enums;
using PcBuilder.SharedKernel;

namespace Moderation.Application.Commands
{
    public record RevokeBanCommand(Guid AdminId, Guid UserId, BanType BanType) : IRequest<Result<bool>>;
}
