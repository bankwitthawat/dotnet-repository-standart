using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Widely.DataModel.ViewModels.Auth.Token
{
    public class RefreshTokenRequest
    {
        [Required]
        public string Token { get; set; }
    }
}
