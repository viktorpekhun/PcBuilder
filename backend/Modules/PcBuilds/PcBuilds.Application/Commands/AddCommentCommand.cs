using MediatR;
using PcBuilder.SharedKernel;

namespace PcBuilds.Application.Commands
{
    public record AddCommentCommand(Guid PcBuildId, Guid UserId, string Text, decimal Rating) : IRequest<Result<Guid>>;
}