using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Widely.DataAccess.DataContext;
using Widely.DataModel.ViewModels.Auth.LogIn;

namespace Widely.API.Infrastructure.Security
{
    public class ModulePermission : TypeFilterAttribute
    {
        public ModulePermission(string module, string permission) : base(typeof(ModulePermissionAuthorizeFilter))
        {
            Arguments = new object[] { module, permission };
        }
    }

    public class ModulePermissionAuthorizeFilter : IAuthorizationFilter
    {
        private readonly WidelystandartContext _context;
        private readonly string _appModule;
        private readonly string _action;

        public ModulePermissionAuthorizeFilter(string AppModule, string action, WidelystandartContext context)
        {
            _context = context;
            _appModule = AppModule;
            _action = action;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (_appModule == "*") return;
            try
            {
                var moduleArr = Array.ConvertAll(_appModule.Split(','), p => p.Trim());


                var currentRefreshToken = context.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "refreshtoken").Value;
                var dbRefreshToken = _context.Authtokens.FirstOrDefault(x => x.Token == currentRefreshToken);

                if (dbRefreshToken == null)
                {
                    context.Result = new UnauthorizedResult(); //401
                    return;
                }

                var appauthorize = JsonSerializer.Deserialize<List<AppModule>>(context.HttpContext.User.Claims
                    .FirstOrDefault(x => x.Type == "appauthorize").Value);


                //bool isAuthorized = HasAuthorize(appauthorize, _AppModule);
                bool isAuthorized = true;

                if (!isAuthorized)
                {
                    context.Result = new ForbidResult(); //403
                }
            }
            catch
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
