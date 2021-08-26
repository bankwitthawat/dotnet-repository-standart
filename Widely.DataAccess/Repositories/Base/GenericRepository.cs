using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Widely.DataAccess.DataContext;

namespace Widely.DataAccess.Repositories.Base
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly DbSet<T> _dbSet;
        public GenericRepository(WidelystandartContext dbContext)
        {
            _dbSet = dbContext.Set<T>();
        }
        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task<bool> AddRangeAsync(IEnumerable<T> entity)
        {
            await _dbSet.AddRangeAsync(entity);
            return true;
        }

        public async Task<List<T>> All(params Expression<Func<T, object>>[] including)
        {
            if (including != null && including.Length > 0)
            {
                var query = _dbSet as IQueryable<T>;

                query = including.Aggregate(query, (current, property) => current.Include(property));

                return await query.AsNoTracking().ToListAsync();
            }

            return await _dbSet.ToListAsync();
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] including)
        {
            if (including != null && including.Length > 0)
            {
                var query = _dbSet as IQueryable<T>;

                query = including.Aggregate(query, (current, property) => current.Include(property));

                return await query.AsNoTracking().FirstOrDefaultAsync(expression);
            }
      
            return await _dbSet.FirstOrDefaultAsync(expression);
        }

        public async Task<List<T>> ListAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] including)
        {
            if (including != null && including.Length > 0)
            {
                var query = _dbSet as IQueryable<T>;

                query = including.Aggregate(query, (current, property) => current.Include(property));

                return await query.AsNoTracking().Where(expression).ToListAsync();
            }
            return await _dbSet.Where(expression).ToListAsync();
        }

        public Task<bool> RemoveAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.FromResult(true);
        }

        public Task<bool> RemoveRangeAsync(IEnumerable<T> entity)
        {
            _dbSet.RemoveRange(entity);
            return Task.FromResult(true);
        }

        public Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.FromResult(entity);
        }
    }
}
