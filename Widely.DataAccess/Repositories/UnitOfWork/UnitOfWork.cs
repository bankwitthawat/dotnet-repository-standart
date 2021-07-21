using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widely.DataAccess.DataContext;
using Widely.DataAccess.Repositories.Base;

namespace Widely.DataAccess.Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly WidelystandartContext _dbContext;

        public UnitOfWork(WidelystandartContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        public Task<int> CommitAsync()
        {
            return _dbContext.SaveChangesAsync();
        }

        public IGenericRepository<T> AsyncRepository<T>() where T : class
        {
            return new GenericRepository<T>(_dbContext);
        }
    }
}
