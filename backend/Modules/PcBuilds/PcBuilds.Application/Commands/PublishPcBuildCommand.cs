using MediatR;
using PcBuilder.SharedKernel;

namespace PcBuilds.Application.Commands
{
    public record PublishPcBuildCommand(Guid PcBuildId, Guid UserId, bool IsPublished) : IRequest<Result<bool>>;
}