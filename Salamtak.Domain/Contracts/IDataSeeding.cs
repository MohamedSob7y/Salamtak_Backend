using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Contracts
{
    public interface IDataSeeding
    {
        Task IntializeAsync();
    }
}
