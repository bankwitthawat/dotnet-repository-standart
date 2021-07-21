using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Widely.DataModel.ViewModels.Auth.Token
{
    public class RefreshToken
    {
        [Key]
        [JsonIgnore]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; }
        public DateTime IssuedOn { get; set; }
        public DateTime? ExpiresOn { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
        public string RequestIp { get; set; }
        public string UserAgent { get; set; }
        public string MachineName { get; set; }
    }

}
