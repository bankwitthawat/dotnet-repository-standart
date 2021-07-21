using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widely.DataAccess.DataContext;
using Widely.DataAccess.Repositories.Base;

namespace Widely.DataAccess.Repositories.Appusers
{
    public class AppusersRepository : GenericRepository<Widely.DataAccess.DataContext.Entities.Appusers>, IAppusersRepository
    {
        public AppusersRepository(WidelystandartContext context) : base(context)
        {

        }
    }
}
