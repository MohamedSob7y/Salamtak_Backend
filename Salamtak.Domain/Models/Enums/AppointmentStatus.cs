using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Models.Enums
{
    public enum AppointmentStatus
    {
        Pending = 1,
        Confirmed = 2,
        Cancelled = 3,
        Completed = 4,
        NoShow = 5
    }
}
