using MediatR;
using PcBuilder.SharedKernel;
using PcBuilds.Application.AutoBuilder;

namespace PcBuilds.Application.Commands
{
    public record AutoBuildCommand(AutoBuildRequestDto Request) : IRequest<Result<AutoBuildResultDto>>;
}
