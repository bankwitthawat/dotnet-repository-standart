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
}
