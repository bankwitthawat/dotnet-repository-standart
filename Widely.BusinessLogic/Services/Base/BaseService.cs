using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Widely.DataAccess.Repositories.UnitOfWork;

namespace Widely.BusinessLogic.Services.Base
{
    public class BaseService
    {
        protected internal IHttpContextAccessor _httpContextAccessor;
        protected internal IUnitOfWork _unitOfWork { get; set; }
        public BaseService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }

        //public LoginUserEntity GetCurrentUserInfo()
        //{
        //    LoginUserEntity result = new LoginUserEntity();
        //    result.Username = this.GetUserName();
        //    if (string.IsNullOrEmpty(result.Username)) return null;
        //    result.RoleId = this.GetRoleId();
        //    result.DepartmentCode = this.GetDepartmentCode();
        //    result.DivisionCode = this.GetDivisionCode();
        //    result.BranchCode = this.GetBranchCode();
        //    result.CompanyCode = this.GetCompanyCode();
        //    result.PositionCode = this.GetPositionCode();
        //    result.RoleLevel = this.GetRoleLevel();
        //    return result;
        //}
        public string GetUserID() => _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        public int GetRoleId() => Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role));
        //public List<AuthorizeAppmoduleEntity> GetCurrentAuthorizeModule() => JsonConvert.DeserializeObject<List<AuthorizeAppmoduleEntity>>(_httpContextAccessor.HttpContext.User.Claims
        //            .FirstOrDefault(x => x.Type == "appauthorize").Value);

        public string GetDepartmentCode() => _httpContextAccessor.HttpContext.User.FindFirst(x => x.Type == "DepartmentCode").Value;
        public string GetDivisionCode() => _httpContextAccessor.HttpContext.User.FindFirst(x => x.Type == "DivisionCode").Value;
        public string GetBranchCode() => _httpContextAccessor.HttpContext.User.FindFirst(x => x.Type == "BranchCode").Value;
        public string GetCompanyCode() => _httpContextAccessor.HttpContext.User.FindFirst(x => x.Type == "CompanyCode").Value;
        public string GetPositionCode() => _httpContextAccessor.HttpContext.User.FindFirst(x => x.Type == "GetPositionCode").Value;
        public int GetRoleLevel() => Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirst(x => x.Type == "RoleLevel").Value);




        public string GetIpAddress() => $"{_httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.MapToIPv4()}";
        public string GetUserAgnet() => $"{_httpContextAccessor.HttpContext.Request.Headers["User-Agent"]}";
        public string GetMachineName() => Dns.GetHostEntry(Dns.GetHostName()).HostName;
    }
}
