using Microsoft.EntityFrameworkCore;
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
        private readonly WidelystandartContext _context;
        public AppusersRepository(WidelystandartContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<DataContext.Entities.Appusers>> GetUserAllRelated()
        {
            var result = await _context.Appusers
                .Include(i => i.Role)
                .ToListAsync();

            return result;
        }
    }
}
