using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Exceptions
{
    public abstract class BaseAppException : Exception
    {
        public int StatusCode { get; }

        protected BaseAppException(string message, int statusCode)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
