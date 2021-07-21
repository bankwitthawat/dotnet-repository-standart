using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Widely.DataModel.ViewModels.Auth.LogIn;
using Widely.BusinessLogic.Services.Auth;
using Widely.DataModel.ViewModels.Auth.Token;
using Widely.DataModel.ViewModels.Auth.Register;
using Widely.BusinessLogic.Services.Base;
using Microsoft.Extensions.Logging;
using NLog;

namespace Widely.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase 
    {
        private readonly ILogger<AuthController> _logger;
        private readonly AuthService _authService;
        public static string _logStatus = "";
        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            this._logger = logger;
            this._authService = authService;
        }

        #region LogIn
        /// <summary>
        /// API endpoint to login a user
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Todogit init --initial-branch=mai
        ///    
        ///     {
        ///        "username": "widelyusername",
        ///        "password": "widelypassword",
        ///     }
        ///
        /// </remarks>
        /// <returns> Unauthorizied if the login fails, The jwt token as string if the login succeded</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LogInRequest request)
        {
            using var _logUsername = MappedDiagnosticsLogicalContext.SetScoped("username", request.Username);
            using var _logAction = MappedDiagnosticsLogicalContext.SetScoped("action", "Login");

            var response = await this._authService.Login(request);

            if (!response.Success)
            {
                using var _logStatusFail = MappedDiagnosticsLogicalContext.SetScoped("status", "Failure");
                _logger.LogDebug("Signed in fail {user}", request.Username);
                return Unauthorized(response);
            }

            using var _logStatusSuccess = MappedDiagnosticsLogicalContext.SetScoped("status", "Success");
            _logger.LogDebug("Signed in success {user}", request.Username);
            return Ok(response);
        }
        #endregion

        #region Refresh Token
        /// <summary>
        /// API endpoint to refresh token
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Todo
        ///     {
        ///        "token": "xxxxxxxxxxxxxxxxxxxxxxx"
        ///     }
        ///
        /// </remarks>
        /// <returns></returns>
        [HttpPost("refreshtoken")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest tokenRequest)
        {
            if (string.IsNullOrEmpty(tokenRequest.Token))
                return Unauthorized(new { message = "Token is required" });

            var response = await this._authService.RefreshToken(tokenRequest.Token);

            if (!response.Success)
            {
                //_logger.LogDebug("Signed in fail {user}", tokenRequest.Username);
                return Unauthorized();
            }



            return Ok(response);

        }
        #endregion

        #region Register
        /// <summary>
        /// API endpoint to refresh token
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Todo
        ///     {
        ///        "username": "widelyuser",
        ///        "password": "widelypassword",
        ///        "fName": "นาย ไวด์ลี่",
        ///        "lName": "เน็กท์"
        ///     }
        ///
        /// </remarks>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            var response = await this._authService.Register(registerRequest);
            return Ok(response);
        }
        #endregion
    }
}
