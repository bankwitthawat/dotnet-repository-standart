using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Widely.DataModel.ViewModels.UserProfile
{
    public class UserProfileRequest
    {
    }

    public class UserProfileForceChangePasswordRequest
    {
        [Required]
        public string password { get; set; }
        [Required]
        public string passwordConfirm { get; set; }
    }
    public class UserProfileChangePasswordRequest
    {

        [Required]
        public string currentPassword { get; set; }
        [Required]
        public string password { get; set; }
        [Required]
        public string passwordConfirm { get; set; }
    }

    public class UserProfileUpdateRequest
    {
        public string fName { get; set; }
        public string lName { get; set; }
        public string email { get; set; }
        public string mobilePhone { get; set; }
        public string birthDate { get; set; }
    }
}
