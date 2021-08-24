using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widely.DataAccess.Repositories.Base;
using Widely.DataModel.ViewModels.Approles.ItemView;
using Widely.DataModel.ViewModels.Auth.LogIn;

namespace Widely.DataAccess.Repositories.Approles
{
    public interface IApprolesRepository : IGenericRepository<Widely.DataAccess.DataContext.Entities.Approles>
    {
        Task<AppRoleItemViewResponse> GetModulePermission();
        Task<AppRoleItemViewResponse> GetModulePermissionByRole(int id);
    }
}
