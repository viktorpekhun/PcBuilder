using MediatR;
using PcBuilder.SharedKernel;
using PcBuilds.Application.Compatibility;
using PcBuilds.Application.Dtos;

namespace PcBuilds.Application.Queries
{
    public record CheckCompatibilityQuery(ComponentsCompatibilityDto Dto)
        : IRequest<Result<BuildCompatibilityReport>>;
}
