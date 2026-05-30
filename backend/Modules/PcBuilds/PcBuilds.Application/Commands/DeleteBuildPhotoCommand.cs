using MediatR;
using PcBuilder.SharedKernel;

namespace PcBuilds.Application.Commands
{
    public record DeleteBuildPhotoCommand(Guid BuildId, Guid UserId) : IRequest<Result<bool>>;
}
