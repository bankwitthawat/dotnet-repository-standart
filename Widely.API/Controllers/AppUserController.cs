using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Widely.BusinessLogic.Services.AppUser;
using Widely.DataModel.ViewModels.Appusers.ListView;
using Widely.DataModel.ViewModels.Common;
using Widely.Infrastructure.Security;

namespace Widely.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AppUserController : ControllerBase
    {
        private readonly AppusersService _appusersService;
        public AppUserController(AppusersService appusersService)
        {
            this._appusersService = appusersService;
        }


        [ModulePermission("USERS", "*")]
        [HttpPost("list")]
        public async Task<IActionResult> GetUserList(SearchCriteria<AppUserListViewRequest> request)
        {
            return Ok();
        }
    }
}
