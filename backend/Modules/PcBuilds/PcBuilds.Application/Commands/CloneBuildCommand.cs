using MediatR;

namespace PcBuilds.Application.Commands
{
    public record CloneBuildCommand(Guid SourceBuildId, Guid UserId) : IRequest<Guid>;
}
