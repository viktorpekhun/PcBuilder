using MediatR;
using PcBuilder.SharedKernel;

namespace PcBuilds.Application.Commands
{
    public record DeleteBuildCommand(Guid PcBuildId, Guid UserId) : IRequest<Result<bool>>;
}