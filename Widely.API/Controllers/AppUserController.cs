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
            var result = await this._appusersService.GetList(request);
            return Ok(result);
        }
        
        /// <summary>
        /// API endpoint to initial all role for dropdownlist
        /// </summary>
        /// <returns></returns>
        [ModulePermission("USERS", "*")]
        [HttpGet("role-list")]
        public async Task<IActionResult> GetRoleToDropDownList()
        {
            var result = await this._appusersService.GetRoleList();
            return Ok(result);
        }


    }
}
