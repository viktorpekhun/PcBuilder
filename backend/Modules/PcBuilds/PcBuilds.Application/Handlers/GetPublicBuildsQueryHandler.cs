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

            var baseQuery = _context.Set<PcBuild>()
                .Where(b => b.IsPublished)
                .AsQueryable();

            if (parameters.Filters.TryGetValue("price_range", out var priceRange) && priceRange.Length >= 2)
            {
                if (decimal.TryParse(priceRange[0], out var minPrice))
                    baseQuery = baseQuery.Where(b => b.Price >= minPrice);
                if (decimal.TryParse(priceRange[1], out var maxPrice))
                    baseQuery = baseQuery.Where(b => b.Price <= maxPrice);
            }

            if (parameters.Filters.TryGetValue("socket", out var sockets) && sockets.Length > 0)
            {
                baseQuery = baseQuery.Where(b => b.Cpu != null && sockets.Contains(b.Cpu.Socket));
            }

            List<Guid> orderedIds;
            int totalCount;

            if (!string.IsNullOrWhiteSpace(parameters.SearchQuery))
            {
                var names = await baseQuery
                    .Select(b => new { b.Id, b.Name, b.Description })
                    .ToListAsync(cancellationToken);

                var ranked = FuzzySearchHelper.RankAndFilter(
                    names,
                    parameters.SearchQuery,
                    x => $"{x.Name} {x.Description}",
                    minScore: 55);

                totalCount = ranked.Count;
                orderedIds = ranked
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .Select(x => x.Id)
                    .ToList();
            }
            else
            {
                totalCount = await baseQuery.CountAsync(cancellationToken);

                var sortedQuery = (parameters.OrderBy?.ToLower()) switch
                {
                    "price" => parameters.Ascending
                        ? baseQuery.OrderBy(b => b.Price)
                        : baseQuery.OrderByDescending(b => b.Price),
                    "averagerating" => parameters.Ascending
                        ? baseQuery.OrderBy(b => b.AverageRating)
                        : baseQuery.OrderByDescending(b => b.AverageRating),
                    "name" => parameters.Ascending
                        ? baseQuery.OrderBy(b => b.Name)
                        : baseQuery.OrderByDescending(b => b.Name),
                    _ => baseQuery.OrderByDescending(b => b.PublishedAt)
                };

                orderedIds = await sortedQuery
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .Select(b => b.Id)
                    .ToListAsync(cancellationToken);
            }

            var projected = await _context.Set<PcBuild>()
                .Where(b => orderedIds.Contains(b.Id))
                .Select(b => new
                {
                    Dto = new PcBuildGalleryDto
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
                        CommentCount = b.Reviews.Count,
                        RatingCount = b.Reviews.Count,
                        Socket = b.Cpu != null ? b.Cpu.Socket : null,
                        PhotoUrl = b.PhotoUrl
                    },
                    RamTypes = b.PcBuild_Rams.Select(r => r.Ram.Type).ToList(),
                    FormFactors = b.PcCase != null
                        ? b.PcCase.PcCaseFormFactors.Select(f => f.Name).ToList()
                        : new List<string>()
                })
                .ToListAsync(cancellationToken);

            // Derived spec tags: socket · first RAM type · first case form-factor (≤3, distinct, non-empty).
            foreach (var row in projected)
            {
                var tags = new List<string>();
                if (!string.IsNullOrWhiteSpace(row.Dto.Socket))
                    tags.Add(row.Dto.Socket!);

                var ramType = row.RamTypes.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));
                if (!string.IsNullOrWhiteSpace(ramType))
                    tags.Add(ramType!);

                var formFactor = row.FormFactors.FirstOrDefault(f => !string.IsNullOrWhiteSpace(f));
                if (!string.IsNullOrWhiteSpace(formFactor))
                    tags.Add(formFactor!);

                row.Dto.Tags = tags
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(3)
                    .ToList();
            }

            var builds = projected.Select(p => p.Dto).ToList();

            // Restore the relevance order for fuzzy results (EF WHERE IN loses ordering)
            var orderedBuilds = orderedIds
                .Select(id => builds.First(b => b.Id == id))
                .ToList();

            return Result.Success(new PagedResponse<PcBuildGalleryDto>(orderedBuilds, totalCount, parameters));
        }
    }
}
