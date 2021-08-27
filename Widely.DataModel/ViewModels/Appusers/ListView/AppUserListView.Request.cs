using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Widely.DataModel.ViewModels.Appusers.ListView
{
    public class AppUserListViewRequest
    {
        public string username { get; set; }
        public string fullName { get; set; }
        public int? roleId { get; set; }
        public bool? isActive { get; set; }
    }

    public class AppUserCreateRequest
    {
        [Required]
        public string username { get; set; }
        [Required]
        public string password { get; set; }
        [Required]
        public int roleId { get; set; }
        public bool forceChangePassword { get; set; }
        public bool isActive { get; set; }
        public string email { get; set; }
        public string fName { get; set; }
        public string lName { get; set; }
        public string mobilePhone { get; set; }
    }

    public class AppUserUpdateRequest
    {
        [Required]
        public int id { get; set; }
        [Required]
        public string username { get; set; }
        [Required]
        public int roleId { get; set; }
        public bool isForceChangePwd { get; set; }
        public bool isActive { get; set; }
        public string email { get; set; }
        public string fName { get; set; }
        public string lName { get; set; }
        public string mobilePhone { get; set; }
        public string birthDate { get; set; }
    }

    public class AppUserUnlockRequest
    {
        [Required]
        public int id { get; set; }
    }
}
