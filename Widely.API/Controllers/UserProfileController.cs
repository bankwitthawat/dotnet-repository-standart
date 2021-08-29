using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Widely.BusinessLogic.Services.UserProfile;
using Widely.DataModel.ViewModels.UserProfile;
using Widely.Infrastructure.Security;

namespace Widely.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserProfileController : ControllerBase
    {
        private readonly UserProfileService _userProfileService;

        public UserProfileController(UserProfileService userProfileService)
        {
            this._userProfileService = userProfileService;
        }

        [ModulePermission("*", "*")]
        [HttpPost("force-change-password")]
        public async Task<IActionResult> ForceChangePassword(UserProfileForceChangePasswordRequest request)
        {
            var result = await this._userProfileService.ForceChangePassword(request);
            return Ok(result);
        }
    }
}
