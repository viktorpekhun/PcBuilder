using MediatR;

namespace PcBuilder.Api.Search
{
    public record GlobalSearchQuery(string Query, int Limit = 5) : IRequest<List<GlobalSearchItemDto>>;
}
