using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Exceptions
{
    public class ConflictException : BaseAppException
    {
        public ConflictException(string message)
            : base(message, 409)
        {
        }
    }
}
