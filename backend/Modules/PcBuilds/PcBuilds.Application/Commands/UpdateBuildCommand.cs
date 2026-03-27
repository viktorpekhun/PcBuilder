using MediatR;
using PcBuilds.Application.Dtos;

namespace PcBuilds.Application.Commands
{
    public record UpdateBuildCommand(Guid PcBuildId, PcBuildInputDto BuildDto) : IRequest<bool>;
}
