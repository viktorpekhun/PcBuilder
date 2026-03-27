using MediatR;

namespace PcBuilds.Application.Commands
{
    public record DeleteBuildCommand(Guid PcBuildId, Guid UserId) : IRequest<bool>;
}
