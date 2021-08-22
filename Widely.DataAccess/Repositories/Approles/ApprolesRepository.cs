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
            var appModule = await _context.Appmodule
                .Where(x => x.IsActive == true).ToListAsync();

            var myPermission = (from q in appModule
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
                                    IsAccess = false,
                                    IsCreate = false,
                                    IsView = false,
                                    IsEdit = false,
                                    IsDelete = false,

                                    IsActive = q.IsActive

                                }).ToList();

            return myPermission;
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
                              IsAccess = subp?.IsAccess == null ? false : true,
                              IsCreate = subp?.IsCreate == null ? false : true,
                              IsView = subp?.IsView == null ? false : true,
                              IsEdit = subp?.IsEdit == null ? false : true,
                              IsDelete = subp?.IsDelete == null ? false : true,

                              IsActive = q.IsActive

                          }).OrderBy(o => o.Sequence).ToList();

            return result;
        }
    }
}
