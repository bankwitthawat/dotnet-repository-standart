using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widely.DataAccess.Repositories.Base;
using Widely.DataModel.ViewModels.Auth.LogIn;

namespace Widely.DataAccess.Repositories.Auth
{
    public interface IAuthRepository : IGenericRepository<Widely.DataAccess.DataContext.Entities.Appusers>
    {
        Task<Widely.DataAccess.DataContext.Entities.Appusers> GetUserRelatedByToken(string token);

        Task<List<AppModule>> GetModulePermissionByRole(int id);

        Task<Widely.DataAccess.DataContext.Entities.Appusers> GetUserRelated(string username);
    }
}
