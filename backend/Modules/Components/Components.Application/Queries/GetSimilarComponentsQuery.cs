using MediatR;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Enums;

namespace Components.Application.Queries
{
    public record GetSimilarComponentsQuery(
        Guid Id,
        ComponentType ComponentType,
        int Count = 4) : IRequest<Result<List<object>>>;
}
