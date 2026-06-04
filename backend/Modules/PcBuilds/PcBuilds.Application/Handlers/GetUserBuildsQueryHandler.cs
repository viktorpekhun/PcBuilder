using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.Dtos;
using PcBuilds.Application.Queries;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class GetUserBuildsQueryHandler : IRequestHandler<GetUserBuildsQuery, Result<List<PcBuildListDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetUserBuildsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<PcBuildListDto>>> Handle(GetUserBuildsQuery request, CancellationToken cancellationToken)
        {
            var builds = await _context.Set<PcBuild>()
                .Where(b => b.UserId == request.UserId)
                .OrderByDescending(b => b.UpdatedAt)
                .Select(b => new PcBuildListDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Price = b.Price,
                    IsPublished = b.IsPublished,
                    ComponentCount =
                        (b.CpuId    != null ? 1 : 0) +
                        (b.GpuId    != null ? 1 : 0) +
                        (b.MotherboardId  != null ? 1 : 0) +
                        (b.CpuCoolerId    != null ? 1 : 0) +
                        (b.PowerSupplyId  != null ? 1 : 0) +
                        (b.PcCaseId != null ? 1 : 0) +
                        b.PcBuild_Rams.Count +
                        b.PcBuild_Ssds.Count +
                        b.PcBuild_Hdds.Count +
                        b.PcBuild_Fans.Count,
                    UpdatedAt = b.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            return Result.Success(builds);
        }
    }
}