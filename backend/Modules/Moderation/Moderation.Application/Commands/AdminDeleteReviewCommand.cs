using MediatR;
using PcBuilder.SharedKernel;

namespace Moderation.Application.Commands
{
    public record AdminDeleteReviewCommand(Guid AdminId, Guid ReviewId, bool NotifyUser = true) : IRequest<Result<bool>>;
}