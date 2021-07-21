using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Widely.DataModel.ViewModels.Auth.LogIn;

namespace Widely.API.Infrastructure.Security
{
    public class AuthorizeAttribute : TypeFilterAttribute
    {
        public AuthorizeAttribute(string item, bool action)
        : base(typeof(AuthorizeActionFilter))
        {
            Arguments = new object[] { item, action };
        }
    }

    public class AuthorizeActionFilter : IAuthorizationFilter
    {
        private readonly string _AppModule;
        private readonly bool _Editable;
        public AuthorizeActionFilter(string AppModule, bool EditablePermission)
        {
            _AppModule = AppModule;
            _Editable = EditablePermission;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (_AppModule == "ALLALLOW") return;
            try
            {
                var currentRefreshToken = context.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "RefreshToken").Value;
                var lastRefreshToken = new
                {

                };

                if (lastRefreshToken == null)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                var appauthorize = JsonSerializer.Deserialize<List<LogInAppModule>>(context.HttpContext.User.Claims
                    .FirstOrDefault(x => x.Type == "appauthorize").Value);


                bool isAuthorized = HasAuthorize(appauthorize, _AppModule, _Editable); // :)

                if (!isAuthorized)
                {
                    context.Result = new StatusCodeResult(500);
                }
            }
            catch
            {
                context.Result = new UnauthorizedResult();
            }
        }

        private bool HasAuthorize(List<LogInAppModule> appauthorize, string AppModuleCode, bool EditablePermission)
        {
            bool hasAuthorizeAccess = appauthorize.Any(x => x.Code == AppModuleCode);
            if (!hasAuthorizeAccess) return false;
            if (EditablePermission == true)
            {
                var appModule = appauthorize.FirstOrDefault(x => x.Code == AppModuleCode);
                if (appModule.Editable == true) return true;
                else return false;
            }
            else
            {
                return true;
            }
        }

    }
}
