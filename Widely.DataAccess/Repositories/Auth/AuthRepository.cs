using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widely.DataAccess.DataContext;
using Widely.DataAccess.Repositories.Base;

namespace Widely.DataAccess.Repositories.Auth
{
    public class AuthRepository : GenericRepository<Widely.DataAccess.DataContext.Entities.Appusers>, IAuthRepository
    {
        private readonly WidelystandartContext _context;
        public AuthRepository(WidelystandartContext context) : base(context)
        {
            _context = context;
        }

        public async Task<DataContext.Entities.Appusers> GetUserRelatedByToken(string token)
        {
            var user = await _context.Appusers
                       .Include(i => i.Authtokens)
                       .SingleOrDefaultAsync(_ => _.Authtokens.Any(t => t.Token == token));

            if (user == null)
            {
                return null;
            }

            return user;
        }
    }
}
