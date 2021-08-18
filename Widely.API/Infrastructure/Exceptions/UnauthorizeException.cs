using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Widely.API.Infrastructure.Exceptions
{
    public class UnauthorizeException : Exception
    {
        public UnauthorizeException() : base() { }

        public UnauthorizeException(string message) : base(message) { }

        public UnauthorizeException(string message, params object[] args)
            : base(String.Format(CultureInfo.CurrentCulture, message, args))
        {
        }
    }
}
