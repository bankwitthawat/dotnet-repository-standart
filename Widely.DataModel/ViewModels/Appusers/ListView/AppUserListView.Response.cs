using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Widely.DataModel.ViewModels.Appusers.ListView
{
    public class AppUserListViewResponse
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string BirthDate { get; set; }
        public string Email { get; set; }
        public string MobilePhone { get; set; }
        public bool IsActive { get; set; }
        public bool? IsForceChangePwd { get; set; }
        public int LoginAttemptCount { get; set; }
        public string LastLogin { get; set; }
        public string LastChangePwd { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
    }
}
