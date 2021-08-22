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
            var appModule = await _context.Appmodule.Where(x => x.IsActive == true).ToListAsync();
            var appPermission = await _context.Apppermission.Where(x => x.RoleId == id).ToListAsync();

            var result = (from q in appModule
                          join per in appPermission on q.Id equals per.ModuleId into g1
                          from subp in g1.DefaultIfEmpty()
                          select new AppModule
                          {
                              ID = q.Id,
                              Title = q.Title,
                              Subtitle = q.Subtitle,
                              Type = q.Type,
                              Icon = q.Icon,
                              Path = q.Path,
                              Sequence = q.Sequence,
                              ParentID = q.ParentId,
                              IsAccess = subp == null ? false : subp.IsAccess,
                              IsCreate = subp == null ? false : subp.IsAccess,
                              IsView = subp == null ? false : subp.IsAccess,
                              IsEdit = subp == null ? false : subp.IsAccess,
                              IsDelete = subp == null ? false : subp.IsAccess,

                              IsActive = q.IsActive

                          }).OrderBy(o => o.Sequence).ToList();

            return result;
        }

        public async Task<DataContext.Entities.Appusers> GetUserRelatedByToken(string token)
        {
            var user = await _context.Appusers
                       .Include(i => i.Authtokens)
                       .Include(i => i.Role)
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

        public async Task<List<int>> FindAncestorById(int moduleId)
        {
            var ancestor = await _context.Appmodule.FromSqlRaw(
                @$"WITH RECURSIVE results AS
                (
                    SELECT *
                    FROM    appmodule
                    WHERE   ID = {moduleId}
                    UNION ALL
                    SELECT  t.*
                    FROM    appmodule t
                            INNER JOIN results r ON r.parentid = t.id
                )
                SELECT *
                FROM    results;
                ").ToListAsync();

            var result = ancestor.Select(x => x.Id).ToList();

            return result;
        }
    }
}
