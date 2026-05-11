using Auth.Domain.Entities;
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
    public class GetBuildCommentsQueryHandler : IRequestHandler<GetBuildCommentsQuery, Result<PagedResponse<CommentDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetBuildCommentsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResponse<CommentDto>>> Handle(GetBuildCommentsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Set<Review>()
                .Where(r => r.PcBuildId == request.PcBuildId)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var comments = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Join(
                    _context.Set<User>(),
                    r => r.UserId,
                    u => u.Id,
                    (r, u) => new CommentDto
                    {
                        Id = r.Id,
                        Text = r.Text ?? string.Empty,
                        Rating = r.Rating,
                        CreatedAt = r.CreatedAt,
                        UserId = r.UserId,
                        Username = u.Username,
                        AvatarUrl = u.AvatarUrl
                    })
                .ToListAsync(cancellationToken);

            var parameters = new ResourceParameters
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result.Success(new PagedResponse<CommentDto>(comments, totalCount, parameters));
        }
    }
}