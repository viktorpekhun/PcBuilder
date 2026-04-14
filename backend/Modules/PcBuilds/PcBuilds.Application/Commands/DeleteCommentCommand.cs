using MediatR;
using PcBuilder.SharedKernel;

namespace PcBuilds.Application.Commands
{
    public record DeleteCommentCommand(Guid CommentId, Guid UserId) : IRequest<Result<bool>>;
}