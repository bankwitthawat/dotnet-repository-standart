using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Widely.DataModel.ViewModels.Appusers.ItemView
{
    public class AppUserItemViewResponse
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Email { get; set; }
        public string MobilePhone { get; set; }
        public bool IsActive { get; set; }
        public bool? IsForceChangePwd { get; set; }
        public int LoginAttemptCount { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? LastChangePwd { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
