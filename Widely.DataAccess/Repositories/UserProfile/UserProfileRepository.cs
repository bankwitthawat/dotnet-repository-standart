using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widely.DataAccess.DataContext;

namespace Widely.DataAccess.Repositories.UserProfile
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly WidelystandartContext _context;
        public UserProfileRepository(WidelystandartContext context)
        {
            _context = context;
        }

        // some query class
    }
}
