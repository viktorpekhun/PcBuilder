using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel.Exceptions;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.Commands;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public DeleteCommentCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = await _context.Set<Comment>()
                .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);

            if (comment == null)
                throw new NotFoundException("Comment not found.");

            if (comment.UserId != request.UserId)
                throw new ForbiddenException("You can only delete your own comments.");

            _context.Remove(comment);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}