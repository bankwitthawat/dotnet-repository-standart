using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widely.DataAccess.DataContext;
using Widely.DataAccess.Repositories.Base;
using Widely.DataModel.ViewModels.Auth.LogIn;

namespace Widely.DataAccess.Repositories.Auth
{
    public class AuthRepository : GenericRepository<Widely.DataAccess.DataContext.Entities.Appusers>, IAuthRepository
    {
        private readonly WidelystandartContext _context;
        public AuthRepository(WidelystandartContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<AppModule>> GetModulePermissionByRole(int id)
        {
            var appPermission = await _context.Apppermission
                .Include(i => i.Module)
                .Include(i => i.Role)
                .Where(x => x.RoleId == id && x.Module.IsActive == true).ToListAsync();

            var myPermission = (from q in appPermission
                                select new AppModule
                                {
                                    ID = q.Module.Id,
                                    Title = q.Module.Title,
                                    Subtitle = q.Module.Subtitle,
                                    Type = q.Module.Type,
                                    Icon = q.Module.Icon,
                                    Path = q.Module.Path,
                                    Sequence = q.Module.Sequence,
                                    ParentID = q.Module.ParentId,
                                    IsAccess = q.IsAccess,
                                    IsCreate = q.IsCreate,
                                    IsView = q.IsView,
                                    IsEdit = q.IsEdit,
                                    IsDelete = q.IsDelete,

                                    IsActive = q.Module.IsActive

                                }).ToList();

            return myPermission;
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

        public async Task<DataContext.Entities.Appusers> GetUserRelated(string username)
        {
            var user = await _context.Appusers
                .Include(i => i.Role)
                .Include(i => i.Authtokens)
                .Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            if (user == null)
            {
                return null;
            }

            return user;
        }
    }
}
