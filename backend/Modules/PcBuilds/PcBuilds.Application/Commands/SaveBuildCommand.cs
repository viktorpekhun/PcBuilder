using MediatR;
using PcBuilds.Application.Dtos;

namespace PcBuilds.Application.Commands
{
    public record SaveBuildCommand(Guid UserId, PcBuildInputDto BuildDto) : IRequest<bool>;
}
