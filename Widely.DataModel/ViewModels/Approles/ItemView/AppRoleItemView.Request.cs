using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Widely.DataModel.ViewModels.Approles.ItemView
{
    public class AppRoleCreateRequest
    {
        [Required]
        public string name { get; set; }
        public string description { get; set; }
        public List<AppModuleList> moduleList { get; set; }
    }


    public class AppRoleUpdateRequest
    {
        [Required]
        public int id { get; set; }
        [Required]
        public string name { get; set; }
        public string description { get; set; }
        public List<AppModuleList> moduleList { get; set; }
    }

    public class AppRoleDeleteRequest
    {
        [Required]
        public int id { get; set; }
    }


    public class AppModuleList
    {
        public int id { get; set; }
        public string title { get; set; }
        public string subtitle { get; set; }
        public string type { get; set; }
        public string icon { get; set; }
        public string path { get; set; }
        public bool isActive { get; set; }
        public int sequence { get; set; }
        public int? parentID { get; set; }
        public bool isAccess { get; set; }
        public bool isCreate { get; set; }
        public bool isEdit { get; set; }
        public bool isView { get; set; }
        public bool isDelete { get; set; }
    }
}
