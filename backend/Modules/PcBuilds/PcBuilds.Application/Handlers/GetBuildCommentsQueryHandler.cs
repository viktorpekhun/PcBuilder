using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel.Filtering;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.Dtos;
using PcBuilds.Application.Queries;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class GetBuildCommentsQueryHandler : IRequestHandler<GetBuildCommentsQuery, PagedResponse<CommentDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetBuildCommentsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<CommentDto>> Handle(GetBuildCommentsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Set<Comment>()
                .Where(c => c.PcBuildId == request.PcBuildId)
                .OrderByDescending(c => c.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var comments = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Join(
                    _context.Set<User>(),
                    c => c.UserId,
                    u => u.Id,
                    (c, u) => new CommentDto
                    {
                        Id = c.Id,
                        Text = c.Text,
                        CreatedAt = c.CreatedAt,
                        UserId = c.UserId,
                        Username = u.Username,
                        AvatarUrl = u.AvatarUrl
                    })
                .ToListAsync(cancellationToken);

            var parameters = new ResourceParameters
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return new PagedResponse<CommentDto>(comments, totalCount, parameters);
        }
    }
}