using MediatR;
using PcBuilder.SharedKernel;
using PcBuilds.Application.Dtos;

namespace PcBuilds.Application.Commands
{
    public record SaveBuildCommand(Guid UserId, PcBuildInputDto BuildDto) : IRequest<Result<bool>>;
}