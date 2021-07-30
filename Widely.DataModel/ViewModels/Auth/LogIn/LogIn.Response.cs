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
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsForceChangePwd { get; set; }
        public int LoginAttemptCount { get; set; }
        public DateTime? LastLogin { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime TokenExpire { get; set; }
        public int TokenTimeoutMins { get; set; }
        public List<AppModule> AppModule { get; set; }
    }


    public class AppModule
    {
        public int? ID { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Type { get; set; }
        public string Icon { get; set; }
        public string Path { get; set; }
        public bool? IsActive { get; set; }
        public int? Sequence { get; set; }
        public int? ParentID { get; set; }

        public bool? IsAccess { get; set; }
        public bool? IsCreate { get; set; }
        public bool? IsEdit { get; set; }
        public bool? IsView { get; set; }
        public bool? IsDelete { get; set; }
        public List<AppModule> Children { get; set; }
        
        //public void Addchildren(AppModule node)
        //{
        //    this.Children.Add(node);
        //}
    }
}
