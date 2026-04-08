using MediatR;

namespace PcBuilds.Application.Commands
{
    public record PublishPcBuildCommand(Guid PcBuildId, Guid UserId, bool IsPublished) : IRequest<bool>;
}