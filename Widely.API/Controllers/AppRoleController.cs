﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Widely.BusinessLogic.Services;
using Widely.BusinessLogic.Services.Auth;
using Widely.BusinessLogic.Services.Base;
using Widely.DataModel.ViewModels.Approles.ItemView;
using Widely.DataModel.ViewModels.Approles.ListView;
using Widely.DataModel.ViewModels.Common;
using Widely.Infrastructure.Security;

namespace Widely.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AppRoleController : ControllerBase
    {
        private readonly BaseService _baseService;
        private readonly ApprolesService _approlesService;
        private readonly Logger _logger;

        public AppRoleController(ApprolesService approlesService)
        {
            this._approlesService = approlesService;
            //_logger = NLog.LogManager.GetCurrentClassLogger();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Todo
        ///    
        ///     {
        ///        "criteria": {
        ///             "name": "string",
        ///             "description": "string"
        ///        },
        ///        "gridCriteria": null,
        ///     }
        ///
        /// </remarks>
        /// <returns></returns>
        [ModulePermission("ROLES", "*")]
        [HttpPost("list")]
        public async Task<IActionResult> GetRoleList(SearchCriteria<AppRoleRequest> request)
        {
            var result = await this._approlesService.GetList(request);
            return Ok(result);
        }


        /// <summary>
        /// API endpoint to initial all module for item view page
        /// </summary>
        /// <param name="roleId"></param>
        /// <remarks>
        /// # Attention
        ///
        ///     1. If you input value is "new" or everything string character, this value will be transform to 0 (int)
        ///     2. If you input value is "1" or everything string number, this value will be transform to number (int)
        ///     
        /// </remarks>
        /// <returns></returns>
        [ModulePermission("ROLES", "*")]
        [HttpGet("module-list")]
        public async Task<IActionResult> GetModuleTreeItem(string roleId)
        {
            var result = await this._approlesService.GetModuleList(roleId);
            return Ok(result);
        }

        [ModulePermission("ROLES", "CREATE")]
        [HttpPost("create-role")]
        public async Task<IActionResult> CreateRole(AppRoleCreateRequest request)
        {
            var result = await this._approlesService.Create(request);
            return Ok(result);
        }

        [ModulePermission("ROLES", "EDIT")]
        [HttpPost("update-role")]
        public async Task<IActionResult> UpdateRole(AppRoleUpdateRequest request)
        {
            var result = await this._approlesService.Update(request);
            return Ok(result);
        }
        
        [ModulePermission("ROLES", "DELETE")]
        [HttpPost("delete-role")]
        public async Task<IActionResult> DeleteRole(AppRoleDeleteRequest request)
        {
            var result = await this._approlesService.Delete(request);
            return Ok(result);
        }


    }
}