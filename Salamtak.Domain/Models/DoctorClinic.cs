using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Salamtak.Domain.Models.Common_Entity;
namespace Salamtak.Domain.Models
{
    public class DoctorClinic : BaseEntity
    {
        public Guid DoctorId { get; set; }

        public Guid ClinicId { get; set; }

       
        public Doctor Doctor { get; set; } = null!;

        public Clinic Clinic { get; set; } = null!;
    }
}
