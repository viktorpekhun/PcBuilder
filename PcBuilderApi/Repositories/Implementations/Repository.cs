using Microsoft.EntityFrameworkCore;
using PcBuilderApi.Data;
using PcBuilderApi.Repositories.Interfaces;
using PcBuilderApi.Utilities.Filtering;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PcBuilderApi.Repositories.Implementations
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly ApplicationDbContext _context;
        private DbSet<TEntity> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>(); ;
        }

        public async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task DeleteAsync(TEntity entity)
        {
            _dbSet?.Remove(entity);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(string? includeProperties = null)
        {
            IQueryable<TEntity> query = _dbSet;
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var property in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }
            return await query.ToListAsync();
        }

        public async Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, string? includeProperties = null)
        {
            IQueryable<TEntity> query = _dbSet;
            query = query.Where(filter);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var property in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter, string? includeProperties = null)
        {
            IQueryable<TEntity> query = _dbSet;
            query = query.Where(filter);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var property in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }
            return await query.ToListAsync();
        }

        public async Task<(IEnumerable<TEntity> items, int totalCount)> GetFilteredAndPagedAsync(
            ResourceParameters parameters,
            Expression<Func<TEntity, bool>> additionalFilter = null,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = _dbSet;

            // Apply additional filter if provided
            if (additionalFilter != null)
            {
                query = query.Where(additionalFilter);
            }

            // Include related entities
            foreach (var includeProperty in includeProperties.Split(
                new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            // Apply search across text fields
            if (!string.IsNullOrWhiteSpace(parameters.SearchQuery))
            {
                query = query.ApplySearch(parameters.SearchQuery);
            }

            // Apply specific filters
            if (parameters.Filters != null && parameters.Filters.Any())
            {
                query = query.ApplyFilters(parameters.Filters);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(parameters.OrderBy))
            {
                query = query.ApplySort(parameters.OrderBy, parameters.Ascending);
            }

            // Apply pagination
            var items = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
