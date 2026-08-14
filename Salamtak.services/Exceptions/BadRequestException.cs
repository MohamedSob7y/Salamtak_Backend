using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Exceptions
{
    public class BadRequestException : BaseAppException
    {
        public BadRequestException(string message)
            : base(message, 400)
        {
        }
    }
}
