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

        [ModulePermission("USERS", "EDIT")]
        [HttpGet("getuserbyid")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var result = await this._appusersService.GetUserById(id);
            return Ok(result);
        }


        [ModulePermission("USERS", "CREATE")]
        [HttpPost("craete-user")]
        public async Task<IActionResult> CraeteUser(AppUserCreateRequest request)
        {
            var result = await this._appusersService.Create(request);
            return Ok(result);
        }

        [ModulePermission("USERS", "EDIT")]
        [HttpPut("update-user")]
        public async Task<IActionResult> UpdateUser(AppUserUpdateRequest request)
        {
            var result = await this._appusersService.Update(request);
            return Ok(result);
        }

        [ModulePermission("USERS", "EDIT")]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(AppUserChangePassowrdRequest request)
        {
            var result = await this._appusersService.ChangePassword(request);
            return Ok(result);
        }

        

        [ModulePermission("USERS", "EDIT")]
        [HttpPut("unlock")]
        public async Task<IActionResult> UnlockUser(AppUserUnlockRequest request)
        {
            var result = await this._appusersService.Unlock(request);
            return Ok(result);
        }
    }
}
