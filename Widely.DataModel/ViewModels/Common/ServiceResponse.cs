using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Widely.DataModel.ViewModels.Common
{
    public class ServiceResponse<T>
    {
        public T Data { get; set; }
        public bool Success { get; set; } = false;
        public string Message { get; set; } = null;
    }
}
