using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widely.DataAccess.Repositories.Base;

namespace Widely.DataAccess.Repositories.Auth
{
    public interface IAuthRepository : IGenericRepository<Widely.DataAccess.DataContext.Entities.Appusers>
    {
        Task<Widely.DataAccess.DataContext.Entities.Appusers> GetUserRelatedByToken(string token);
    }
}
