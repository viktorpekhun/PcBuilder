using MediatR;

namespace PcBuilds.Application.Commands
{
    public record DeleteCommentCommand(Guid CommentId, Guid UserId) : IRequest<bool>;
}