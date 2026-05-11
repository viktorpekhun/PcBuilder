using MediatR;
using PcBuilder.SharedKernel;

namespace PcBuilds.Application.Commands
{
    public record CloneBuildCommand(Guid SourceBuildId, Guid UserId) : IRequest<Result<Guid>>;
}