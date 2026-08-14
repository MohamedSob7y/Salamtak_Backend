using Salamtak.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
