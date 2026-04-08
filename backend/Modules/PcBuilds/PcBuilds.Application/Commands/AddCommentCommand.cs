using MediatR;

namespace PcBuilds.Application.Commands
{
    public record AddCommentCommand(Guid PcBuildId, Guid UserId, string Text) : IRequest<Guid>;
}