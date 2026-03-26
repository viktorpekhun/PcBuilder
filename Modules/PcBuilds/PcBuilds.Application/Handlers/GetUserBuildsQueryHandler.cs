using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.Dtos;
using PcBuilds.Application.Queries;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class GetUserBuildsQueryHandler : IRequestHandler<GetUserBuildsQuery, List<PcBuildListDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetUserBuildsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PcBuildListDto>> Handle(GetUserBuildsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var builds = await _context.Set<PcBuild>()
                    .Where(b => b.UserId == request.UserId)
                    .OrderByDescending(b => b.UpdatedAt)
                    .Select(b => new PcBuildListDto
                    {
                        Id = b.Id,
                        Name = b.Name,
                        Price = b.Price,
                        UpdatedAt = b.UpdatedAt
                    })
                    .ToListAsync(cancellationToken);

                return builds;
            }
            catch
            {
                return new List<PcBuildListDto>();
            }
        }
    }
}
