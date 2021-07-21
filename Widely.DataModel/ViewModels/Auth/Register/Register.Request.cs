using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Widely.DataModel.ViewModels.Auth.Register
{
    public class RegisterRequest
    {
        [Required]
        public string username { get; set; }

        [Required]
        public string password { get; set; }

        [Required]
        public string fName { get; set; }

        [Required]
        public string lName { get; set; }
    }
}
