using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widely.DataAccess.Repositories.Base;

namespace Widely.DataAccess.Repositories.Appusers
{
    public interface IAppusersRepository : IGenericRepository<Widely.DataAccess.DataContext.Entities.Appusers>
    {
        
    }
}
