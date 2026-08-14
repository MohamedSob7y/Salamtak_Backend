using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Exceptions
{
    public class AppValidationException : BaseAppException
    {
        public List<string> Errors { get; }

        public AppValidationException(IEnumerable<string> errors)
            : base("Validation failed.", 400)
        {
            Errors = errors.ToList();
        }
    }
}
