using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Widely.DataModel.ViewModels.Auth.LogIn
{
    public class LogInResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public int? RoleId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsForceChangePwd { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime TokenExpire { get; set; }
        public int TokenTimeoutMins { get; set; }
    }


    public class LogInAppModule
    {
        public long AppRoleModuleId { get; set; }
        public string Code { get; set; }
        public string MainModuleCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Editable { get; set; }
        public string Icon { get; set; }
        public string Path { get; set; }
        public int? ItemSort { get; set; }
    }
}
