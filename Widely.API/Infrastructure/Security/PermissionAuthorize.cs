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

        private bool hasPermission = false;

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

                this.HasAuthorize(appauthorize, moduleArr, _action);
                bool isAuthorized = this.hasPermission;

                if (!isAuthorized)
                {
                    context.Result = new ForbidResult("Access Denied."); //403
                }
            }
            catch
            {
                context.Result = new UnauthorizedResult();
            }
        }

        private void HasAuthorize(List<AppModule> appModules, string[] appModuleName, string permission)
        {
            if (this.hasPermission || permission == "*")
            {
                return;
            }

            foreach (var item in appModules)
            {
                bool hasAccess = appModuleName.Any(x => x == item.Title) && item.IsAccess == true;

                if (hasAccess)
                {
                    this.hasPermission = permission.ToUpper() switch
                    {
                        "CREATE" when item.IsCreate == true => true,
                        "EDIT" when item.IsEdit == true => true,
                        "VIEW" when item.IsView == true => true,
                        "DELETE" when item.IsDelete == true => true,
                        _ => false
                    };

                    if (this.hasPermission)
                        return;

                }

                if (item.Children != null)
                {
                    HasAuthorize(item.Children, appModuleName, permission);
                }
            }

            return;
        }
    }
}
