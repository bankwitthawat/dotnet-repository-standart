using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widely.DataAccess.Repositories.Base;

namespace Widely.DataAccess.Repositories.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task<int> CommitAsync();
        IGenericRepository<T> AsyncRepository<T>() where T : class;
    }
}
