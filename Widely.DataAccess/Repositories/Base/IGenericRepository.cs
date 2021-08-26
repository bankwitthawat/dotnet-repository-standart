using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Widely.DataAccess.Repositories.Base
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> AddAsync(T entity);

        Task<bool> AddRangeAsync(IEnumerable<T> entity);

        Task<T> UpdateAsync(T entity);

        Task<bool> RemoveAsync(T entity);

        Task<bool> RemoveRangeAsync(IEnumerable<T> entity);

        Task<T> GetAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] including);

        Task<List<T>> ListAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] including);

        Task<List<T>> All(params Expression<Func<T, object>>[] including);

    }
}
