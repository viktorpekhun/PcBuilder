using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace PcBuilder.SharedKernel.Persistence
{
    public interface IApplicationDbContext
    {
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        EntityEntry Remove(object entity);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
