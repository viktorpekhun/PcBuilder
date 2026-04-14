using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Filtering;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.Dtos;
using PcBuilds.Application.Queries;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class GetPublicBuildsQueryHandler : IRequestHandler<GetPublicBuildsQuery, Result<PagedResponse<PcBuildGalleryDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetPublicBuildsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResponse<PcBuildGalleryDto>>> Handle(GetPublicBuildsQuery request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            var query = _context.Set<PcBuild>()
                .Where(b => b.IsPublished)
                .Include(b => b.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(parameters.SearchQuery))
            {
                var search = parameters.SearchQuery.ToLower();
                query = query.Where(b =>
                    b.Name.ToLower().Contains(search) ||
                    (b.Description != null && b.Description.ToLower().Contains(search)));
            }

            if (parameters.Filters.TryGetValue("price_range", out var priceRange) && priceRange.Length >= 2)
            {
                if (decimal.TryParse(priceRange[0], out var minPrice))
                    query = query.Where(b => b.Price >= minPrice);
                if (decimal.TryParse(priceRange[1], out var maxPrice))
                    query = query.Where(b => b.Price <= maxPrice);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = (parameters.OrderBy?.ToLower()) switch
            {
                "price" => parameters.Ascending
                    ? query.OrderBy(b => b.Price)
                    : query.OrderByDescending(b => b.Price),
                "averagerating" => parameters.Ascending
                    ? query.OrderBy(b => b.AverageRating)
                    : query.OrderByDescending(b => b.AverageRating),
                "name" => parameters.Ascending
                    ? query.OrderBy(b => b.Name)
                    : query.OrderByDescending(b => b.Name),
                _ => query.OrderByDescending(b => b.PublishedAt)
            };

            var builds = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(b => new PcBuildGalleryDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    Price = b.Price,
                    AverageRating = b.AverageRating,
                    PublishedAt = b.PublishedAt,
                    Username = b.User.Username,
                    AvatarUrl = b.User.AvatarUrl,
                    ComponentCount =
                        (b.CpuId != null ? 1 : 0) +
                        (b.GpuId != null ? 1 : 0) +
                        (b.MotherboardId != null ? 1 : 0) +
                        (b.CpuCoolerId != null ? 1 : 0) +
                        (b.PowerSupplyId != null ? 1 : 0) +
                        (b.PcCaseId != null ? 1 : 0) +
                        b.PcBuild_Rams.Count +
                        b.PcBuild_Ssds.Count +
                        b.PcBuild_Hdds.Count +
                        b.PcBuild_Fans.Count,
                    CommentCount = b.Reviews.Count
                })
                .ToListAsync(cancellationToken);

            return Result.Success(new PagedResponse<PcBuildGalleryDto>(builds, totalCount, parameters));
        }
    }
}