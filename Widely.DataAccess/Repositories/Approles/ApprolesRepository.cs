using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widely.DataAccess.DataContext;
using Widely.DataAccess.Repositories.Base;
using Widely.DataModel.ViewModels.Auth.LogIn;

namespace Widely.DataAccess.Repositories.Approles
{
    public class ApprolesRepository : GenericRepository<Widely.DataAccess.DataContext.Entities.Approles>, IApprolesRepository
    {
        private readonly WidelystandartContext _context;
        public ApprolesRepository(WidelystandartContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<AppModule>> GetModulePermission()
        {
            var appPermission = await _context.Apppermission
                 .Include(i => i.Module)
                 .Include(i => i.Role)
                 .Where(x => x.Module.IsActive == true).ToListAsync();

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
    }
}
